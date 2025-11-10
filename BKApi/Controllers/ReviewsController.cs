using Application.Interfaces;
using Application.Request.Review;
using Application.Response.Common;
using Application.Response.Review;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        #region Public Endpoints

        /// <summary>
        /// Lấy danh sách reviews của tour (Public - Chỉ approved)
        /// </summary>
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> GetTourReviews(
            int tourId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reviewService.GetTourReviewsAsync(tourId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê reviews của tour (Public)
        /// </summary>
        [HttpGet("tour/{tourId}/statistics")]
        public async Task<ActionResult<ReviewStatisticsResponse>> GetTourStatistics(int tourId)
        {
            try
            {
                var stats = await _reviewService.GetTourReviewStatisticsAsync(tourId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tổng quan reviews của tour (Public)
        /// </summary>
        [HttpGet("tour/{tourId}/summary")]
        public async Task<ActionResult<ReviewSummaryResponse>> GetTourSummary(int tourId)
        {
            try
            {
                var summary = await _reviewService.GetTourReviewSummaryAsync(tourId);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết review (Public)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound(new { message = $"Review with ID {id} not found" });

            return Ok(review);
        }

        /// <summary>
        /// Mark review as helpful (Public - không cần đăng nhập)
        /// </summary>
        [HttpPost("{id}/helpful")]
        public async Task<ActionResult> MarkHelpful(int id)
        {
            var result = await _reviewService.MarkReviewHelpfulAsync(id);

            if (!result)
                return NotFound(new { message = $"Review with ID {id} not found" });

            return Ok(new { message = "Marked as helpful", reviewId = id });
        }

        /// <summary>
        /// Tìm kiếm reviews (Public - chỉ approved)
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> SearchReviews(
            [FromQuery] ReviewSearchRequest request)
        {
            // Force approved status for public search
            request.Status = ReviewStatus.Approved;

            try
            {
                var result = await _reviewService.SearchReviewsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Customer Endpoints (Authenticated)

        /// <summary>
        /// Lấy danh sách reviews của user hiện tại
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<List<ReviewResponse>>> GetMyReviews()
        {
            var userId = GetCurrentUserId();
            var reviews = await _reviewService.GetUserReviewsAsync(userId);
            return Ok(reviews);
        }

        /// <summary>
        /// Kiểm tra user đã review tour chưa
        /// </summary>
        [HttpGet("check/{tourId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<object>> CheckReviewed(int tourId)
        {
            var userId = GetCurrentUserId();
            var hasReviewed = await _reviewService.HasUserReviewedTourAsync(userId, tourId);
            return Ok(new { tourId, hasReviewed });
        }

        /// <summary>
        /// Lấy review của booking
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Customer,Admin,Manager")]
        public async Task<ActionResult<ReviewResponse>> GetReviewByBooking(int bookingId)
        {
            var review = await _reviewService.GetReviewByBookingAsync(bookingId);
            if (review == null)
                return NotFound(new { message = "Review not found for this booking" });

            return Ok(review);
        }

        /// <summary>
        /// Tạo review mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ReviewResponse>> CreateReview(
            [FromBody] CreateReviewRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var review = await _reviewService.CreateReviewAsync(userId, request);
                return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the review", detail = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật review (Owner only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ReviewResponse>> UpdateReview(
            int id,
            [FromBody] UpdateReviewRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var review = await _reviewService.UpdateReviewAsync(userId, id, request);
                return Ok(review);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the review", detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa review (Owner only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _reviewService.DeleteReviewAsync(userId, id);

                if (!result)
                    return NotFound(new { message = $"Review with ID {id} not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the review", detail = ex.Message });
            }
        }

        #endregion

        #region Admin/Manager/Staff Endpoints

        /// <summary>
        /// Lấy danh sách reviews pending (Staff+)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> GetPendingReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _reviewService.GetPendingReviewsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Đếm số reviews pending (Staff+)
        /// </summary>
        [HttpGet("pending/count")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<object>> GetPendingCount()
        {
            var count = await _reviewService.GetPendingReviewCountAsync();
            return Ok(new { pendingCount = count });
        }

        /// <summary>
        /// Duyệt review (Staff+)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> ApproveReview(int id)
        {
            try
            {
                var approvedBy = GetCurrentUserId();
                var result = await _reviewService.ApproveReviewAsync(id, approvedBy);

                if (!result)
                    return NotFound(new { message = $"Review with ID {id} not found" });

                return Ok(new { message = "Review approved successfully", reviewId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while approving the review", detail = ex.Message });
            }
        }

        /// <summary>
        /// Từ chối review (Staff+)
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> RejectReview(int id)
        {
            try
            {
                var result = await _reviewService.RejectReviewAsync(id);

                if (!result)
                    return NotFound(new { message = $"Review with ID {id} not found" });

                return Ok(new { message = "Review rejected successfully", reviewId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while rejecting the review", detail = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tất cả reviews (bao gồm pending/rejected) (Manager+)
        /// </summary>
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> AdminSearchReviews(
            [FromQuery] ReviewSearchRequest request)
        {
            try
            {
                var result = await _reviewService.SearchReviewsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy reviews của user cụ thể (Manager+)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<ReviewResponse>>> GetUserReviews(int userId)
        {
            var reviews = await _reviewService.GetUserReviewsAsync(userId);
            return Ok(reviews);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            return userId;
        }

        #endregion
    }
}
