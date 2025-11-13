using Application.Interfaces;
using Application.Request.Guid;
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
    public class GuidesController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuidesController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        /// <summary>
        /// Lấy danh sách TẤT CẢ guides (cho Admin gán vào tour)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GuideListResponse>>> GetAllGuides()
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return Ok(guides);
        }

        /// <summary>
        /// Lấy danh sách guides ACTIVE (đang hoạt động)
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<GuideListResponse>>> GetActiveGuides()
        {
            var guides = await _guideService.GetActiveGuidesAsync();
            return Ok(guides);
        }
        [HttpPut("user/{userId}")]
        [Authorize(Roles = "Guide,Admin")]
        [ProducesResponseType(typeof(BaseResponse<GuideProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateGuideProfileRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only update their own guide profile unless they are Admin
                if (currentUserId != userId && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse
                    {
                        Success = false,
                        Message = "You do not have permission to update this guide profile"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var response = await _guideService.UpdateProfileAsync(userId, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while updating guide profile"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm guides theo tên hoặc ngôn ngữ
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<GuideListResponse>>> SearchGuides(
            [FromQuery] string? keyword = null,
            [FromQuery] string? language = null)
        {
            var guides = await _guideService.SearchGuidesAsync(keyword, language);
            return Ok(guides);
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
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Guide,Admin")]
        public async Task<ActionResult<GuideDetailResponse>> GetGuideProfileByUserId(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own profile unless they are Admin
                if (currentUserId != userId && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse
                    {
                        Success = false,
                        Message = "You do not have permission to view this profile"
                    });
                }

                var guide = await _guideService.GetGuideByUserIdAsync(userId);

                if (guide == null)
                    return NotFound(new { message = $"Guide profile for user {userId} not found" });

                return Ok(guide);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while fetching guide profile"
                });
            }
        }
        /// <summary>
        /// Lấy chi tiết guide theo ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GuideDetailResponse>> GetGuideById(int id)
        {
            var guide = await _guideService.GetGuideByIdAsync(id);

            if (guide == null)
                return NotFound(new { message = $"Guide with ID {id} not found" });

            return Ok(guide);
        }
    }
}
