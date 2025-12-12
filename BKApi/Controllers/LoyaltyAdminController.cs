using Application.Interfaces;
using Application.Request.Loyalty;
using Application.Response.Loyalty;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LoyaltyAdminController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyAdminController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        /// <summary>
        /// [ADMIN] Xem tổng quan điểm thưởng của tất cả users
        /// </summary>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(AdminLoyaltyOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoyaltyOverview(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? tierFilter = null)
        {
            var overview = await _loyaltyService.GetAdminLoyaltyOverviewAsync(
                page,
                pageSize,
                searchTerm,
                tierFilter);

            return Ok(new
            {
                success = true,
                data = overview.Users,
                statistics = overview.Statistics,
                totalCount = overview.TotalCount,
                page = overview.Page,
                pageSize = overview.PageSize,
                message = "Lấy tổng quan điểm thưởng thành công"
            });
        }

        /// <summary>
        /// [ADMIN] Xem chi tiết điểm thưởng của một user cụ thể
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(AdminUserLoyaltyDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserLoyaltyDetail(int userId)
        {
            try
            {
                var detail = await _loyaltyService.GetAdminUserLoyaltyDetailAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = detail,
                    message = "Lấy chi tiết điểm thưởng thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// [ADMIN] Xem lịch sử điểm của một user cụ thể
        /// </summary>
        [HttpGet("user/{userId}/history")]
        [ProducesResponseType(typeof(PointsHistoryListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPointsHistory(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
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
        /// [ADMIN] Xem tất cả lịch sử điểm thưởng của hệ thống (tất cả users)
        /// </summary>
        [HttpGet("all-history")]
        [ProducesResponseType(typeof(AdminAllPointsHistoryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPointsHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? transactionType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var history = await _loyaltyService.GetAdminAllPointsHistoryAsync(
                page,
                pageSize,
                transactionType,
                fromDate,
                toDate);

            return Ok(new
            {
                success = true,
                data = history.Data,
                totalCount = history.TotalCount,
                page = history.Page,
                pageSize = history.PageSize,
                message = "Lịch sử điểm của tất cả users lấy thành công"
            });
        }
        /// <summary>
        /// [ADMIN] Cộng/trừ điểm thủ công cho user (admin adjustment)
        /// </summary>
        [HttpPost("adjust-points")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustUserPoints([FromBody] AdminAdjustPointsRequest request)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

                await _loyaltyService.AdminAdjustPointsAsync(
                    request.UserId,
                    request.Points,
                    request.Reason,
                    adminEmail);

                return Ok(new
                {
                    success = true,
                    message = request.Points > 0
                        ? $"Đã cộng {request.Points:N0} điểm thành công"
                        : $"Đã trừ {Math.Abs(request.Points):N0} điểm thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
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

