using Application.Interfaces;
using Application.Request.Booking;
using Application.Request.Tour;
using Application.Response.Tour;
using Application.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/tours/{tourId}/departures")]
    public class TourDeparturesController : ControllerBase
    {
        private readonly ITourDepartureService _departureService;
        private readonly ILogger<TourDeparturesController> _logger;

        public TourDeparturesController(
            ITourDepartureService departureService,
            ILogger<TourDeparturesController> logger)
        {
            _departureService = departureService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách ngày khởi hành của tour (Public)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<DepartureListResponse>> GetDepartures(
            int tourId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] bool availableOnly = true)
        {
            try
            {
                var response = availableOnly
                    ? await _departureService.GetAvailableDeparturesAsync(tourId, fromDate)
                    : await _departureService.GetAllDeparturesAsync(tourId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departures for tour {TourId}", tourId);
                return StatusCode(500, new DepartureListResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving departures"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết ngày khởi hành (Public)
        /// </summary>
        [HttpGet("{departureId}")]
        public async Task<ActionResult<BaseResponse<TourDepartureResponse>>> GetDeparture(
            int tourId,
            int departureId)
        {
            try
            {
                var response = await _departureService.GetDepartureByIdAsync(departureId);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departure {DepartureId}", departureId);
                return StatusCode(500, new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving departure"
                });
            }
        }

        /// <summary>
        /// Kiểm tra slot còn trống (Public)
        /// </summary>
        [HttpGet("{departureId}/availability")]
        public async Task<ActionResult<object>> CheckAvailability(
            int tourId,
            int departureId,
            [FromQuery] int numberOfGuests = 1)
        {
            try
            {
                var isAvailable = await _departureService.CheckAvailabilityAsync(
                    departureId,
                    numberOfGuests);

                return Ok(new
                {
                    departureId,
                    numberOfGuests,
                    isAvailable,
                    message = isAvailable
                        ? "Departure is available"
                        : "Not enough slots available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Tạo ngày khởi hành mới (Admin/Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse<TourDepartureResponse>>> CreateDeparture(
            int tourId,
            [FromBody] CreateDepartureDto request)
        {
            try
            {
                var response = await _departureService.CreateDepartureAsync(tourId, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(
                    nameof(GetDeparture),
                    new { tourId, departureId = response.Data!.Id },
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating departure");
                return StatusCode(500, new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = "An error occurred while creating departure"
                });
            }
        }

        /// <summary>
        /// Tạo nhiều ngày khởi hành (Admin/Manager)
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DepartureListResponse>> BulkCreateDepartures(
            int tourId,
            [FromBody] List<CreateDepartureDto> requests)
        {
            try
            {
                var response = await _departureService.BulkCreateDeparturesAsync(tourId, requests);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating departures");
                return StatusCode(500, new DepartureListResponse
                {
                    Success = false,
                    Message = "An error occurred while creating departures"
                });
            }
        }

        /// <summary>
        /// Tạo departures theo pattern (Admin/Manager)
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DepartureListResponse>> GenerateDepartures(
            int tourId,
            [FromBody] BulkCreateDeparturesRequest request)
        {
            try
            {
                request.TourId = tourId;
                var response = await _departureService.GenerateDeparturesAsync(request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating departures");
                return StatusCode(500, new DepartureListResponse
                {
                    Success = false,
                    Message = "An error occurred while generating departures"
                });
            }
        }

        /// <summary>
        /// Cập nhật ngày khởi hành (Admin/Manager)
        /// </summary>
        [HttpPut("{departureId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse<TourDepartureResponse>>> UpdateDeparture(
            int tourId,
            int departureId,
            [FromBody] CreateDepartureDto request)
        {
            try
            {
                var response = await _departureService.UpdateDepartureAsync(departureId, request);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating departure");
                return StatusCode(500, new BaseResponse<TourDepartureResponse>
                {
                    Success = false,
                    Message = "An error occurred while updating departure"
                });
            }
        }

        /// <summary>
        /// Xóa ngày khởi hành (Admin/Manager)
        /// </summary>
        [HttpDelete("{departureId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse>> DeleteDeparture(
            int tourId,
            int departureId)
        {
            try
            {
                var response = await _departureService.DeleteDepartureAsync(departureId);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting departure");
                return StatusCode(500, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting departure"
                });
            }
        }
    }
}