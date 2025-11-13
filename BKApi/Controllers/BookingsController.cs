using Application.Interfaces;
using Application.Request.Booking;
using Application.Response.Booking;
using Application.Response.Guide;
using Application.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BookingListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var response = await _bookingService.GetAllAsync(page, pageSize, status);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings list");
                return StatusCode(StatusCodes.Status500InternalServerError, new BookingListResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var response = await _bookingService.GetByIdAsync(id);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (response.Data!.UserId != currentUserId && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "You do not have permission to view this booking"
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving booking"
                });
            }
        }

        /// <summary>
        /// Get booking by booking code
        /// </summary>
        [HttpGet("code/{bookingCode}")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByBookingCode(string bookingCode)
        {
            try
            {
                var response = await _bookingService.GetByBookingCodeAsync(bookingCode);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (response.Data!.UserId != currentUserId && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "You do not have permission to view this booking"
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by code: {BookingCode}", bookingCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving booking"
                });
            }
        }

        /// <summary>
        /// Get current user's bookings
        /// </summary>
        [HttpGet("my-bookings")]
        [ProducesResponseType(typeof(BookingListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var response = await _bookingService.GetByUserIdAsync(currentUserId, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(StatusCodes.Status500InternalServerError, new BookingListResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Get bookings by tour ID
        /// </summary>
        [HttpGet("tour/{tourId}")]
        [Authorize(Roles = "Admin,Guide")]
        [ProducesResponseType(typeof(BookingListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTourId(
            int tourId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _bookingService.GetByTourIdAsync(tourId, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour bookings");
                return StatusCode(StatusCodes.Status500InternalServerError, new BookingListResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Get bookings by guide ID
        /// </summary>
        [HttpGet("guide/{guideId}")]
        [Authorize(Roles = "Admin,Guide")]
        [ProducesResponseType(typeof(BookingListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByGuideId(
            int guideId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _bookingService.GetByGuideIdAsync(guideId, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guide bookings");
                return StatusCode(StatusCodes.Status500InternalServerError, new BookingListResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Create new booking
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var currentUserId = GetCurrentUserId();
                var response = await _bookingService.CreateAsync(currentUserId, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating booking"
                });
            }
        }

        /// <summary>
        /// Update booking
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                // Check booking ownership
                var bookingResponse = await _bookingService.GetByIdAsync(id);
                if (!bookingResponse.Success)
                {
                    return NotFound(bookingResponse);
                }

                var currentUserId = GetCurrentUserId();
                if (bookingResponse.Data!.UserId != currentUserId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "You can only update your own bookings"
                    });
                }

                var response = await _bookingService.UpdateAsync(id, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating booking"
                });
            }
        }

        /// <summary>
        /// Update booking status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                var response = await _bookingService.UpdateStatusAsync(id, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating booking status"
                });
            }
        }

        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                // Check booking ownership
                var bookingResponse = await _bookingService.GetByIdAsync(id);
                if (!bookingResponse.Success)
                {
                    return NotFound(bookingResponse);
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (bookingResponse.Data!.UserId != currentUserId && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "You can only cancel your own bookings"
                    });
                }

                var response = await _bookingService.CancelAsync(id, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = "An error occurred while cancelling booking"
                });
            }
        }

        /// <summary>
        /// Delete booking (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _bookingService.DeleteAsync(id);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting booking"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("available-guides")]
        [ProducesResponseType(typeof(GuideAvailabilityListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableGuides(
            [FromQuery] int tourId,
            [FromQuery] DateTime tourDate)
        {
            try
            {
                if (tourId <= 0)
                {
                    return BadRequest(new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "Invalid tour ID"
                    });
                }

                if (tourDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "Tour date must be in the future"
                    });
                }

                var response = await _bookingService.GetAvailableGuidesForBookingAsync(tourId, tourDate);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available guides for tour {TourId} on {TourDate}",
                    tourId, tourDate);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "An error occurred while retrieving available guides"
                    });
            }
        }
        /// <summary>
        /// Get available guides for authenticated user's booking (alternative endpoint)
        /// </summary>
        [HttpGet("tours/{tourId}/available-guides")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GuideAvailabilityListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableGuidesForTour(
            int tourId,
            [FromQuery] DateTime tourDate)
        {
            try
            {
                if (tourDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "Tour date must be in the future"
                    });
                }

                var response = await _bookingService.GetAvailableGuidesForBookingAsync(tourId, tourDate);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available guides for tour {TourId} on {TourDate}",
                    tourId, tourDate);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "An error occurred while retrieving available guides"
                    });
            }
        }
        [HttpPatch("{id}/assign-guide")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignGuide(
    int id,
    [FromBody] AssignGuideRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                var response = await _bookingService.AssignGuideAsync(id, request.GuideId);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning guide to booking: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "An error occurred while assigning guide"
                    });
            }
        }

        /// <summary>
        /// Remove guide from booking (Admin only)
        /// </summary>
        [HttpDelete("{id}/guide")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(BaseResponse<BookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveGuide(int id)
        {
            try
            {
                var response = await _bookingService.AssignGuideAsync(id, null);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing guide from booking: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "An error occurred while removing guide"
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

            #endregion
        }
    }

