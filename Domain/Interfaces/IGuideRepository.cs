using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGuideRepository : IGenericRepository<Guide>
    {
        Task<Guide?> GetByIdAsync(int id);
        Task<Guide?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Guide>> GetAllAsync();
        Task<IEnumerable<Guide>> GetActiveGuidesAsync();
        Task<IEnumerable<Guide>> GetTopRatedGuidesAsync(int take = 10);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task UpdateGuideRatingAsync(int guideId);
        Task<IEnumerable<Guide>> SearchGuidesAsync(
            string? keyword = null,
            string? language = null,
            int? minExperience = null,
            decimal? minRating = null);
    }
}
