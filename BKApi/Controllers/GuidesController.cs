using Application.Interfaces;
using Application.Response.Guide;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Lấy chi tiết guide theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GuideDetailResponse>> GetGuideById(int id)
        {
            var guide = await _guideService.GetGuideByIdAsync(id);

            if (guide == null)
                return NotFound(new { message = $"Guide with ID {id} not found" });

            return Ok(guide);
        }
    }
}
