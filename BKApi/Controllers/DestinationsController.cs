using Application.Interfaces;
using Application.Request.Destination;
using Application.Response.Common;
using Application.Response.Destination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DestinationsController : ControllerBase
    {
        private readonly IDestinationService _destinationService;

        public DestinationsController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        /// <summary>
        /// Lấy danh sách destination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<DestinationListResponse>>> GetDestinations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _destinationService.GetAllDestinationsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy destination active
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<DestinationListResponse>>> GetActiveDestinations()
        {
            var destinations = await _destinationService.GetActiveDestinationsAsync();
            return Ok(destinations);
        }

        /// <summary>
        /// Lấy destination phổ biến
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<List<DestinationListResponse>>> GetPopularDestinations(
            [FromQuery] int take = 10)
        {
            var destinations = await _destinationService.GetPopularDestinationsAsync(take);
            return Ok(destinations);
        }

        /// <summary>
        /// Lấy destination nổi bật
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<List<DestinationListResponse>>> GetFeaturedDestinations(
            [FromQuery] int take = 10)
        {
            var destinations = await _destinationService.GetFeaturedDestinationsAsync(take);
            return Ok(destinations);
        }

        /// <summary>
        /// Tìm kiếm destination
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<DestinationListResponse>>> SearchDestinations(
            [FromQuery] DestinationSearchRequest request)
        {
            var result = await _destinationService.SearchDestinationsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết destination theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DestinationDetailResponse>> GetDestinationById(int id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound(new { message = $"Destination with ID {id} not found" });

            return Ok(destination);
        }

        /// <summary>
        /// Lấy chi tiết destination theo slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<DestinationDetailResponse>> GetDestinationBySlug(string slug)
        {
            var destination = await _destinationService.GetDestinationBySlugAsync(slug);
            if (destination == null)
                return NotFound(new { message = $"Destination with slug '{slug}' not found" });

            return Ok(destination);
        }

        /// <summary>
        /// Lấy danh sách quốc gia
        /// </summary>
        [HttpGet("countries")]
        public async Task<ActionResult<List<string>>> GetCountries()
        {
            var countries = await _destinationService.GetAllCountriesAsync();
            return Ok(countries);
        }

        /// <summary>
        /// Tạo destination mới (Admin, Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DestinationDetailResponse>> CreateDestination(
            [FromBody] CreateDestinationRequest request)
        {
            try
            {
                var destination = await _destinationService.CreateDestinationAsync(request);
                return CreatedAtAction(nameof(GetDestinationById), new { id = destination.Id }, destination);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật destination (Admin, Manager)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DestinationDetailResponse>> UpdateDestination(
            int id,
            [FromBody] UpdateDestinationRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest(new { message = "ID mismatch" });

                var destination = await _destinationService.UpdateDestinationAsync(id, request);
                return Ok(destination);
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
        /// Xóa destination (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDestination(int id)
        {
            var result = await _destinationService.DeleteDestinationAsync(id);
            if (!result)
                return NotFound(new { message = $"Destination with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Toggle featured status (Admin, Manager)
        /// </summary>
        [HttpPatch("{id}/toggle-featured")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ToggleFeatured(int id)
        {
            var result = await _destinationService.ToggleFeaturedAsync(id);
            if (!result)
                return NotFound(new { message = $"Destination with ID {id} not found" });

            return Ok(new { message = "Featured status updated" });
        }

        /// <summary>
        /// Cập nhật thống kê (Admin, Manager, Staff)
        /// </summary>
        [HttpPost("{id}/update-statistics")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> UpdateStatistics(int id)
        {
            await _destinationService.UpdateDestinationStatisticsAsync(id);
            return Ok(new { message = "Statistics updated" });
        }

        /// <summary>
        /// Cập nhật display order (Admin, Manager)
        /// </summary>
        [HttpPatch("display-order")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> UpdateDisplayOrder(
            [FromBody] Dictionary<int, int> displayOrders)
        {
            await _destinationService.UpdateDisplayOrderAsync(displayOrders);
            return Ok(new { message = "Display order updated" });
        }
    }
}
