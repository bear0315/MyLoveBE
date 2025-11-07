using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IFavoriteRepository : IGenericRepository<Favorite>
    {
        Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Tour>> GetFavoriteToursByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> IsFavoriteAsync(int userId, int tourId);
        Task<Favorite?> GetByUserAndTourAsync(int userId, int tourId);
        Task ToggleFavoriteAsync(int userId, int tourId);
        Task<int> CountByTourIdAsync(int tourId);
    }
}
