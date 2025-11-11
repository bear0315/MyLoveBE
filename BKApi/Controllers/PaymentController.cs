using Application.Interfaces;
using Application.Request.Payment;
using Application.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;


        public PaymentController(
            IVnPayService vnPayService,
            IBookingService bookingService,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _bookingService = bookingService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Create VNPay payment URL for booking
        /// </summary>
        [HttpPost("vnpay/create")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentUrlRequest request)
        {
            try
            {
                // Validate booking exists and belongs to user
                var bookingResponse = await _bookingService.GetByIdAsync(request.BookingId);

                if (!bookingResponse.Success || bookingResponse.Data == null)
                {
                    return NotFound(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Booking not found"
                    });
                }

                var booking = bookingResponse.Data;
                var currentUserId = GetCurrentUserId();

                // Check if user owns this booking
                if (booking.UserId != currentUserId)
                {
                    return Forbid();
                }

                // Check if booking is already paid
                if (booking.PaymentStatus == "Paid")
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Booking is already paid"
                    });
                }

                // Check if booking is cancelled
                if (booking.Status == "Cancelled")
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Cannot pay for cancelled booking"
                    });
                }

                // Create VNPay payment request
                var vnPayRequest = new VnPaymentRequest
                {
                    BookingId = booking.Id,
                    BookingCode = booking.BookingCode,
                    Amount = booking.TotalAmount,
                    OrderDescription = $"Thanh toan dat tour {booking.TourName} - Ma dat cho: {booking.BookingCode}",
                    CustomerName = booking.CustomerName,
                    CreatedDate = DateTime.Now
                };

                var ipAddress = GetClientIpAddress();
                var paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest, ipAddress);

                _logger.LogInformation("Created VNPay payment URL for booking {BookingCode}", booking.BookingCode);

                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = "Payment URL created successfully",
                    Data = paymentUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment URL");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while creating payment URL"
                });
            }
        }

        /// <summary>
        /// VNPay payment callback (Return URL) - Called when user returns from VNPay
        /// </summary>
        // API/Controllers/PaymentController.cs - VnPayCallback method

        [HttpGet("vnpay/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            try
            {
                _logger.LogInformation("VNPay callback received");

                var response = _vnPayService.ProcessPaymentCallback(Request.Query);

                if (response.Success)
                {
                    // Update booking payment status
                    var updatePaymentRequest = new Application.Request.Booking.UpdatePaymentRequest
                    {
                        PaymentStatus = "Paid",
                        PaymentTransactionId = response.TransactionId,
                        PaymentDate = response.PaymentDate
                    };

                    var bookingResponse = await _bookingService.GetByBookingCodeAsync(response.OrderId);

                    if (bookingResponse.Success && bookingResponse.Data != null)
                    {
                        await _bookingService.UpdatePaymentAsync(bookingResponse.Data.Id, updatePaymentRequest);

                        _logger.LogInformation("Payment successful for booking {BookingCode}, Transaction: {TransactionId}",
                            response.OrderId, response.TransactionId);
                    }

                    // THAY ĐỔI: Redirect về frontend URL (React)
                    // Giả sử React app chạy ở localhost:3000
                    var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
                    var frontendUrl = $"{frontendBaseUrl}/payment-success?bookingCode={response.OrderId}&transactionId={response.TransactionId}";
                    return Redirect(frontendUrl);
                }
                else
                {
                    _logger.LogWarning("Payment failed for booking {BookingCode}, Response Code: {ResponseCode}",
                        response.OrderId, response.VnPayResponseCode);

                    // THAY ĐỔI: Redirect về frontend failure page
                    var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
                    var frontendUrl = $"{frontendBaseUrl}/payment-failure?bookingCode={response.OrderId}&message={Uri.EscapeDataString(response.Message)}";
                    return Redirect(frontendUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
                var frontendUrl = $"{frontendBaseUrl}/payment-error";
                return Redirect(frontendUrl);
            }
        }
        /// <summary>
        /// VNPay IPN (Instant Payment Notification) - Called by VNPay server
        /// </summary>
        [HttpGet("vnpay/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIPN()
        {
            try
            {
                _logger.LogInformation("VNPay IPN received");

                var response = _vnPayService.ProcessPaymentCallback(Request.Query);

                if (response.Success)
                {
                    // Update booking payment status
                    var updatePaymentRequest = new Application.Request.Booking.UpdatePaymentRequest
                    {
                        PaymentStatus = "Paid",
                        PaymentTransactionId = response.TransactionId,
                        PaymentDate = response.PaymentDate
                    };

                    var bookingResponse = await _bookingService.GetByBookingCodeAsync(response.OrderId);

                    if (bookingResponse.Success && bookingResponse.Data != null)
                    {
                        // Check if already processed
                        if (bookingResponse.Data.PaymentStatus != "Paid")
                        {
                            await _bookingService.UpdatePaymentAsync(bookingResponse.Data.Id, updatePaymentRequest);

                            _logger.LogInformation("IPN: Payment confirmed for booking {BookingCode}, Transaction: {TransactionId}",
                                response.OrderId, response.TransactionId);
                        }
                        else
                        {
                            _logger.LogInformation("IPN: Payment already processed for booking {BookingCode}", response.OrderId);
                        }

                        // Return success to VNPay
                        return Ok(new { RspCode = "00", Message = "Confirm Success" });
                    }
                    else
                    {
                        _logger.LogWarning("IPN: Booking not found {BookingCode}", response.OrderId);
                        return Ok(new { RspCode = "01", Message = "Order not found" });
                    }
                }
                else
                {
                    _logger.LogWarning("IPN: Payment failed {BookingCode}, Response Code: {ResponseCode}",
                        response.OrderId, response.VnPayResponseCode);

                    return Ok(new { RspCode = "00", Message = "Confirm Success" }); // Still return success to acknowledge receipt
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        /// <summary>
        /// Check payment status for booking
        /// </summary>
        [HttpGet("status/{bookingId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckPaymentStatus(int bookingId)
        {
            try
            {
                var bookingResponse = await _bookingService.GetByIdAsync(bookingId);

                if (!bookingResponse.Success || bookingResponse.Data == null)
                {
                    return NotFound(new BaseResponse
                    {
                        Success = false,
                        Message = "Booking not found"
                    });
                }

                var currentUserId = GetCurrentUserId();
                if (bookingResponse.Data.UserId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(new BaseResponse<object>
                {
                    Success = true,
                    Message = "Payment status retrieved successfully",
                    Data = new
                    {
                        bookingResponse.Data.BookingCode,
                        bookingResponse.Data.PaymentStatus,
                        bookingResponse.Data.PaymentMethod,
                        bookingResponse.Data.PaymentTransactionId,
                        bookingResponse.Data.PaymentDate,
                        bookingResponse.Data.TotalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while checking payment status"
                });
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        private string GetClientIpAddress()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }

        #endregion
    }
}
