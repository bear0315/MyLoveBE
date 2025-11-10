using Application.Interfaces;
using Application.Request.Favorite;
using Application.Response.Common;
using Application.Response.Favorite;
using Application.Response.Tour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Lấy danh sách tour yêu thích của user hiện tại
        /// </summary>
        [HttpGet("my-favorites")]
        public async Task<ActionResult<List<FavoriteResponse>>> GetMyFavorites()
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }

        /// <summary>
        /// Lấy danh sách tour yêu thích với phân trang
        /// </summary>
        [HttpGet("my-favorite-tours")]
        public async Task<ActionResult<PagedResult<TourListResponse>>> GetMyFavoriteTours(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.GetUserFavoriteToursAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Kiểm tra xem tour có trong favorites không
        /// </summary>
        [HttpGet("check/{tourId}")]
        public async Task<ActionResult<object>> CheckFavorite(int tourId)
        {
            var userId = GetCurrentUserId();
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, tourId);
            return Ok(new { isFavorite });
        }

        /// <summary>
        /// Kiểm tra nhiều tour cùng lúc
        /// </summary>
        [HttpPost("check-multiple")]
        public async Task<ActionResult<Dictionary<int, bool>>> CheckMultipleFavorites(
            [FromBody] List<int> tourIds)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.CheckMultipleFavoritesAsync(userId, tourIds);
            return Ok(result);
        }

        /// <summary>
        /// Thêm tour vào favorites
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FavoriteResponse>> AddFavorite(
            [FromBody] AddFavoriteRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var favorite = await _favoriteService.AddFavoriteAsync(userId, request.TourId);
                return CreatedAtAction(nameof(CheckFavorite), new { tourId = request.TourId }, favorite);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa tour khỏi favorites
        /// </summary>
        [HttpDelete("{tourId}")]
        public async Task<ActionResult> RemoveFavorite(int tourId)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.RemoveFavoriteAsync(userId, tourId);

            if (!result)
                return NotFound(new { message = "Favorite not found" });

            return NoContent();
        }

        /// <summary>
        /// Toggle favorite (thêm nếu chưa có, xóa nếu đã có)
        /// </summary>
        [HttpPost("toggle/{tourId}")]
        public async Task<ActionResult<object>> ToggleFavorite(int tourId)
        {
            var userId = GetCurrentUserId();
            await _favoriteService.ToggleFavoriteAsync(userId, tourId);

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, tourId);
            return Ok(new { isFavorite, message = isFavorite ? "Added to favorites" : "Removed from favorites" });
        }

        /// <summary>
        /// Lấy số lượng user đã favorite tour này (Public)
        /// </summary>
        [HttpGet("tour/{tourId}/count")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFavoriteCount(int tourId)
        {
            var count = await _favoriteService.GetFavoriteCountByTourAsync(tourId);
            return Ok(new { tourId, favoriteCount = count });
        }

        /// <summary>
        /// Lấy danh sách favorites của user cụ thể (Admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<FavoriteResponse>>> GetUserFavorites(int userId)
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }

        // Helper method to get current user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }
    }
}
