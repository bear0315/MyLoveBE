using Application.Interfaces;
using Application.Request.Booking;
using Application.Response.Loyalty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        /// <summary>
        /// Lấy thông tin điểm thưởng và hạng thành viên của user hiện tại
        /// </summary>
        [HttpGet("my-loyalty")]
        [ProducesResponseType(typeof(LoyaltyInfoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyLoyaltyInfo()
        {
            var userId = GetCurrentUserId();
            var loyaltyInfo = await _loyaltyService.GetLoyaltyInfoAsync(userId);

            return Ok(new
            {
                success = true,
                data = loyaltyInfo,
                message = "Thông tin điểm thưởng lấy thành công"
            });
        }

        /// <summary>
        /// Lấy lịch sử giao dịch điểm của user hiện tại
        /// </summary>
        [HttpGet("my-history")]
        [ProducesResponseType(typeof(PointsHistoryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPointsHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var history = await _loyaltyService.GetPointsHistoryAsync(userId, page, pageSize);

            return Ok(new
            {
                success = true,
                data = history.Data,
                totalCount = history.TotalCount,
                page = history.Page,
                pageSize = history.PageSize,
                message = "Lịch sử điểm lấy thành công"
            });
        }

        /// <summary>
        /// Tính preview giảm giá cho một booking (trước khi tạo)
        /// </summary>
        [HttpPost("calculate-discount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculateDiscount([FromBody] DiscountCalculationRequest request)
        {
            var userId = GetCurrentUserId();
            var loyaltyInfo = await _loyaltyService.GetLoyaltyInfoAsync(userId);

            var discount = _loyaltyService.CalculateDiscount(request.TotalAmount, loyaltyInfo.CurrentTier);
            var finalAmount = request.TotalAmount - discount;
            var pointsToEarn = _loyaltyService.CalculatePointsEarned(finalAmount);

            return Ok(new
            {
                success = true,
                data = new
                {
                    originalAmount = $"{request.TotalAmount:N0} VND",
                    memberTier = loyaltyInfo.CurrentTier.ToString(),
                    discountPercentage = loyaltyInfo.DiscountPercentage,
                    discountAmount = $"{discount:N0} VND",
                    finalAmount = $"{finalAmount:N0} VND",
                    pointsToEarn = $"{pointsToEarn:N0} điểm"
                },
                message = "Tính toán giảm giá thành công"
            });
        }

        /// <summary>
        /// Preview việc đổi điểm cho booking (trước khi tạo)
        /// </summary>
        [HttpPost("preview-redeem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PreviewPointsRedemption([FromBody] PointsRedemptionRequest request)
        {
            var userId = GetCurrentUserId();
            var loyaltyInfo = await _loyaltyService.GetLoyaltyInfoAsync(userId);

            // Validate
            if (request.PointsToRedeem % 100 != 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Số điểm phải là bội số của 100 (100 điểm = 1,000 VND)"
                });
            }

            if (loyaltyInfo.CurrentPoints < request.PointsToRedeem)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Không đủ điểm. Bạn có {loyaltyInfo.CurrentPoints:N0} điểm"
                });
            }

            // Tính toán
            var memberDiscount = _loyaltyService.CalculateDiscount(request.BookingAmount, loyaltyInfo.CurrentTier);
            var amountAfterMemberDiscount = request.BookingAmount - memberDiscount;

            var maxRedeemablePoints = _loyaltyService.CalculateMaxRedeemablePoints(amountAfterMemberDiscount);
            var actualPointsToRedeem = Math.Min(request.PointsToRedeem, maxRedeemablePoints);

            // 100 points = 1,000 VND
            var pointsDiscount = (decimal)actualPointsToRedeem * 10;
            var finalAmount = amountAfterMemberDiscount - pointsDiscount;

            // Tính điểm sẽ được cộng sau khi hoàn thành tour
            var pointsToEarn = _loyaltyService.CalculatePointsEarned(finalAmount);

            return Ok(new
            {
                success = true,
                data = new
                {
                    originalAmount = $"{request.BookingAmount:N0} VND",
                    memberTier = loyaltyInfo.CurrentTier.ToString(),
                    memberDiscount = $"{memberDiscount:N0} VND",
                    amountAfterMemberDiscount = $"{amountAfterMemberDiscount:N0} VND",

                    currentPoints = $"{loyaltyInfo.CurrentPoints:N0} điểm",
                    pointsToRedeem = $"{actualPointsToRedeem:N0} điểm",
                    pointsDiscount = $"{pointsDiscount:N0} VND",

                    finalAmount = $"{finalAmount:N0} VND",
                    remainingPoints = $"{(loyaltyInfo.CurrentPoints - actualPointsToRedeem):N0} điểm",
                    pointsToEarn = $"{pointsToEarn:N0} điểm",

                    maxRedeemablePoints = $"{maxRedeemablePoints:N0} điểm",
                    maxRedeemableValue = $"{(maxRedeemablePoints * 10):N0} VND",

                    exchangeRate = "100 điểm = 1,000 VND",
                    note = actualPointsToRedeem < request.PointsToRedeem
                        ? $"Chỉ có thể đổi tối đa {maxRedeemablePoints:N0} điểm (50% giá trị booking)"
                        : null
                },
                message = "Tính toán thành công"
            });
        }

        /// <summary>
        /// Lấy thông tin các hạng thành viên
        /// </summary>
        [HttpGet("tiers")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMemberTiers()
        {
            var tiers = _loyaltyService.GetMemberTierInfo();

            return Ok(new
            {
                success = true,
                data = tiers,
                message = "Thông tin các hạng thành viên"
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }
    }
}

