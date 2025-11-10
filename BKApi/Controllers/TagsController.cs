using Application.Interfaces;
using Application.Request.Tag;
using Application.Response.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Lấy tất cả tags
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TagResponse>>> GetAllTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        /// <summary>
        /// Lấy tags phổ biến
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<List<TagResponse>>> GetPopularTags(
            [FromQuery] int take = 20)
        {
            var tags = await _tagService.GetPopularTagsAsync(take);
            return Ok(tags);
        }

        /// <summary>
        /// Lấy tags có tour
        /// </summary>
        [HttpGet("with-tours")]
        public async Task<ActionResult<List<TagResponse>>> GetTagsWithTours(
            [FromQuery] int minTourCount = 1)
        {
            var tags = await _tagService.GetTagsWithToursAsync(minTourCount);
            return Ok(tags);
        }

        /// <summary>
        /// Tìm kiếm tags
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<TagResponse>>> SearchTags(
            [FromQuery] string? keyword = null)
        {
            var tags = await _tagService.SearchTagsAsync(keyword);
            return Ok(tags);
        }

        /// <summary>
        /// Lấy tag dictionary (id -> name)
        /// </summary>
        [HttpGet("dictionary")]
        public async Task<ActionResult<Dictionary<int, string>>> GetTagDictionary()
        {
            var dictionary = await _tagService.GetTagDictionaryAsync();
            return Ok(dictionary);
        }

        /// <summary>
        /// Lấy usage counts của tất cả tags
        /// </summary>
        [HttpGet("usage-counts")]
        public async Task<ActionResult<Dictionary<int, int>>> GetUsageCounts()
        {
            var counts = await _tagService.GetTagUsageCountsAsync();
            return Ok(counts);
        }

        /// <summary>
        /// Lấy chi tiết tag theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDetailResponse>> GetTagById(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound(new { message = $"Tag with ID {id} not found" });

            return Ok(tag);
        }

        /// <summary>
        /// Lấy chi tiết tag theo slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<TagDetailResponse>> GetTagBySlug(string slug)
        {
            var tag = await _tagService.GetTagBySlugAsync(slug);
            if (tag == null)
                return NotFound(new { message = $"Tag with slug '{slug}' not found" });

            return Ok(tag);
        }

        /// <summary>
        /// Tạo tag mới (Admin, Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TagDetailResponse>> CreateTag(
            [FromBody] CreateTagRequest request)
        {
            try
            {
                var tag = await _tagService.CreateTagAsync(request);
                return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật tag (Admin, Manager)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TagDetailResponse>> UpdateTag(
            int id,
            [FromBody] UpdateTagRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest(new { message = "ID mismatch" });

                var tag = await _tagService.UpdateTagAsync(id, request);
                return Ok(tag);
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
        /// Xóa tag (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (!result)
                return NotFound(new { message = $"Tag with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Tự động tạo tags (Admin, Manager, Staff)
        /// </summary>
        [HttpPost("bulk-create")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<List<TagResponse>>> BulkCreateTags(
            [FromBody] List<string> tagNames)
        {
            try
            {
                var tags = await _tagService.GetOrCreateTagsAsync(tagNames);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
