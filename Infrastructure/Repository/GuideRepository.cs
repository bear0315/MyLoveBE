using Domain.Entities.Enums;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure.Repository
{
    public class GuideRepository : GenericRepository<Guide>, IGuideRepository
    {
        public GuideRepository(DatabaseContext context) : base(context) { }

        public async Task<Guide?> GetByIdAsync(int id)
        {
            return await _context.Guides
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
        }

        public async Task<Guide?> GetByUserIdAsync(int userId)
        {
            return await _context.Guides
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.UserId == userId && !g.IsDeleted);
        }

        public async Task<IEnumerable<Guide>> GetAllAsync()
        {
            return await _context.Guides
                .Include(g => g.User)
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Guide>> GetActiveGuidesAsync()
        {
            return await _context.Guides
                .Include(g => g.User)
                .Where(g => !g.IsDeleted && g.Status == GuideStatus.Active)
                .OrderByDescending(g => g.AverageRating)
                .ToListAsync();
        }

        public async Task<IEnumerable<Guide>> GetTopRatedGuidesAsync(int take = 10)
        {
            return await _context.Guides
                .Include(g => g.User)
                .Where(g => !g.IsDeleted && g.Status == GuideStatus.Active)
                .OrderByDescending(g => g.AverageRating)
                .ThenByDescending(g => g.TotalReviews)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Guides
                .AnyAsync(g => g.Id == id && !g.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Guides.Where(g => g.Email == email && !g.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(g => g.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateGuideRatingAsync(int guideId)
        {
            var guide = await _context.Guides.FindAsync(guideId);
            if (guide == null) return;

            var reviews = await _context.GuideReviews
                .Where(r => r.GuideId == guideId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            guide.TotalReviews = reviews.Count;
            guide.AverageRating = reviews.Any()
                ? (decimal)reviews.Average(r => r.Rating)
                : 0;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Guide>> SearchGuidesAsync(
            string? keyword = null,
            string? language = null,
            int? minExperience = null,
            decimal? minRating = null)
        {
            var query = _context.Guides
                .Include(g => g.User)
                .Where(g => !g.IsDeleted && g.Status == GuideStatus.Active);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(g =>
                    g.FullName.Contains(keyword) ||
                    (g.Bio != null && g.Bio.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(g =>
                    g.Languages != null && g.Languages.Contains(language));
            }

            if (minExperience.HasValue)
            {
                query = query.Where(g => g.ExperienceYears >= minExperience.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(g => g.AverageRating >= minRating.Value);
            }

            return await query
                .OrderByDescending(g => g.AverageRating)
                .ThenByDescending(g => g.ExperienceYears)
                .ToListAsync();
        }
    }
}
