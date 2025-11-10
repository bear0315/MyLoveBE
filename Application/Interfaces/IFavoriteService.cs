using Application.Response.Common;
using Application.Response.Favorite;
using Application.Response.Tour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<FavoriteResponse>> GetUserFavoritesAsync(int userId);
        Task<PagedResult<TourListResponse>> GetUserFavoriteToursAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> IsFavoriteAsync(int userId, int tourId);
        Task<FavoriteResponse> AddFavoriteAsync(int userId, int tourId);
        Task<bool> RemoveFavoriteAsync(int userId, int tourId);
        Task<bool> ToggleFavoriteAsync(int userId, int tourId);
        Task<int> GetFavoriteCountByTourAsync(int tourId);
        Task<Dictionary<int, bool>> CheckMultipleFavoritesAsync(int userId, List<int> tourIds);
    }
}
