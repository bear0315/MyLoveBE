using Application.Interfaces;
using Application.Request.Tour;
using Application.Response.Common;
using Application.Response.Tour;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;

        public ToursController(ITourService tourService)
        {
            _tourService = tourService;
        }

        // Public endpoints - không cần authentication

        /// <summary>
        /// Lấy danh sách tour với phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<TourListResponse>>> GetTours(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _tourService.GetAllToursAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm và filter tour
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<TourListResponse>>> SearchTours(
            [FromQuery] TourSearchRequest request)
        {
            var result = await _tourService.SearchToursAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết tour theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TourDetailResponse>> GetTourById(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null)
                return NotFound(new { message = $"Tour with ID {id} not found" });

            return Ok(tour);
        }

        /// <summary>
        /// Lấy chi tiết tour theo slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<TourDetailResponse>> GetTourBySlug(string slug)
        {
            var tour = await _tourService.GetTourBySlugAsync(slug);
            if (tour == null)
                return NotFound(new { message = $"Tour with slug '{slug}' not found" });

            return Ok(tour);
        }

        /// <summary>
        /// Lấy danh sách tour nổi bật
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<List<TourListResponse>>> GetFeaturedTours(
            [FromQuery] int take = 10)
        {
            var tours = await _tourService.GetFeaturedToursAsync(take);
            return Ok(tours);
        }

        /// <summary>
        /// Lấy danh sách tour phổ biến
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<List<TourListResponse>>> GetPopularTours(
            [FromQuery] int take = 10)
        {
            var tours = await _tourService.GetPopularToursAsync(take);
            return Ok(tours);
        }

        /// <summary>
        /// Lấy danh sách tour liên quan
        /// </summary>
        [HttpGet("{id}/related")]
        public async Task<ActionResult<List<TourListResponse>>> GetRelatedTours(
            int id,
            [FromQuery] int take = 5)
        {
            var tours = await _tourService.GetRelatedToursAsync(id, take);
            return Ok(tours);
        }

        /// <summary>
        /// Lấy tour theo destination
        /// </summary>
        [HttpGet("destination/{destinationId}")]
        public async Task<ActionResult<List<TourListResponse>>> GetToursByDestination(
            int destinationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var tours = await _tourService.GetToursByDestinationAsync(destinationId, pageNumber, pageSize);
            return Ok(tours);
        }

        /// <summary>
        /// Lấy tour theo category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<TourListResponse>>> GetToursByCategory(
            TourCategory category,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var tours = await _tourService.GetToursByCategoryAsync(category, pageNumber, pageSize);
            return Ok(tours);
        }

        /// <summary>
        /// Kiểm tra tính khả dụng của tour
        /// </summary>
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<object>> CheckAvailability(
            int id,
            [FromQuery] DateTime tourDate,
            [FromQuery] int numberOfGuests)
        {
            var isAvailable = await _tourService.CheckTourAvailabilityAsync(id, tourDate, numberOfGuests);
            return Ok(new { isAvailable, tourDate, numberOfGuests });
        }

        /// <summary>
        /// Lấy danh sách ngày available
        /// </summary>
        [HttpGet("{id}/available-dates")]
        public async Task<ActionResult<List<DateTime>>> GetAvailableDates(
            int id,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var dates = await _tourService.GetAvailableDatesAsync(id, fromDate, toDate);
            return Ok(dates);
        }

        // Admin/Manager/Staff endpoints - cần authentication & authorization

        /// <summary>
        /// Tạo tour mới (Admin, Manager, Staff)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<TourDetailResponse>> CreateTour(
            [FromBody] CreateTourRequest request)
        {
            try
            {
                var tour = await _tourService.CreateTourAsync(request);
                return CreatedAtAction(nameof(GetTourById), new { id = tour.Id }, tour);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật tour (Admin, Manager, Staff)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<TourDetailResponse>> UpdateTour(
            int id,
            [FromBody] UpdateTourRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest(new { message = "ID mismatch" });

                var tour = await _tourService.UpdateTourAsync(id, request);
                return Ok(tour);
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
        /// Xóa tour (Admin, Manager)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteTour(int id)
        {
            var result = await _tourService.DeleteTourAsync(id);
            if (!result)
                return NotFound(new { message = $"Tour with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Toggle featured status (Admin, Manager)
        /// </summary>
        [HttpPatch("{id}/toggle-featured")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ToggleFeatured(int id)
        {
            var result = await _tourService.ToggleFeaturedAsync(id);
            if (!result)
                return NotFound(new { message = $"Tour with ID {id} not found" });

            return Ok(new { message = "Featured status updated successfully" });
        }

        /// <summary>
        /// Cập nhật status tour (Admin, Manager)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> UpdateStatus(
            int id,
            [FromBody] TourStatus status)
        {
            var result = await _tourService.UpdateStatusAsync(id, status);
            if (!result)
                return NotFound(new { message = $"Tour with ID {id} not found" });

            return Ok(new { message = "Status updated successfully" });
        }

        /// <summary>
        /// Cập nhật thống kê tour (Admin, Manager, Staff)
        /// </summary>
        [HttpPost("{id}/update-statistics")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> UpdateStatistics(int id)
        {
            await _tourService.UpdateTourStatisticsAsync(id);
            return Ok(new { message = "Statistics updated successfully" });
        }

        /// <summary>
        /// Cập nhật rating tour (Admin, Manager, Staff)
        /// </summary>
        [HttpPost("{id}/update-rating")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> UpdateRating(int id)
        {
            await _tourService.UpdateTourRatingAsync(id);
            return Ok(new { message = "Rating updated successfully" });
        }

        /// <summary>
        /// Lấy thống kê tổng quan (Admin, Manager)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TourStatisticsDto>> GetStatistics()
        {
            var stats = await _tourService.GetTourStatisticsAsync();
            return Ok(stats);
        }

        // Bulk operations

        /// <summary>
        /// Cập nhật status hàng loạt (Admin, Manager)
        /// </summary>
        [HttpPatch("bulk/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> BulkUpdateStatus(
            [FromBody] BulkUpdateStatusRequest request)
        {
            var result = await _tourService.BulkUpdateStatusAsync(request);
            if (!result)
                return BadRequest(new { message = "Bulk update failed" });

            return Ok(new { message = $"Updated status for {request.TourIds.Count} tours" });
        }

        /// <summary>
        /// Cập nhật featured hàng loạt (Admin, Manager)
        /// </summary>
        [HttpPatch("bulk/featured")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> BulkUpdateFeatured(
            [FromBody] BulkUpdateFeaturedRequest request)
        {
            var result = await _tourService.BulkUpdateFeaturedAsync(request.TourIds, request.IsFeatured);
            if (!result)
                return BadRequest(new { message = "Bulk update failed" });

            return Ok(new { message = $"Updated featured status for {request.TourIds.Count} tours" });
        }

        /// <summary>
        /// Xóa hàng loạt (Admin only)
        /// </summary>
        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkDelete([FromBody] List<int> tourIds)
        {
            var result = await _tourService.BulkDeleteToursAsync(tourIds);
            if (!result)
                return BadRequest(new { message = "Bulk delete failed" });

            return Ok(new { message = $"Deleted {tourIds.Count} tours" });
        }
    }
}
