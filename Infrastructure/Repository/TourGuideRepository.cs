using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class TourGuideRepository : GenericRepository<TourGuide>, ITourGuideRepository
    {
        private readonly DatabaseContext _dbContext;

        public TourGuideRepository(DatabaseContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<TourGuide>> GetByTourIdAsync(int tourId)
        {
            return await _dbSet
                .Include(tg => tg.Guide)
                    .ThenInclude(g => g.User)
                .Include(tg => tg.Tour)
                .Where(tg => tg.TourId == tourId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourGuide>> GetByGuideIdAsync(int guideId)
        {
            return await _dbSet
                .Include(tg => tg.Tour)
                    .ThenInclude(t => t.Images)
                .Where(tg => tg.GuideId == guideId)
                .ToListAsync();
        }

        public async Task<TourGuide?> GetDefaultGuideForTourAsync(int tourId)
        {
            return await _dbSet
                .Include(tg => tg.Guide)
                    .ThenInclude(g => g.User)
                .FirstOrDefaultAsync(tg => tg.TourId == tourId && tg.IsDefault);
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var tourGuides = await _dbSet
                .Where(tg => tg.TourId == tourId)
                .ToListAsync();

            if (tourGuides.Any())
            {
                _dbSet.RemoveRange(tourGuides);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkAddAsync(List<TourGuide> tourGuides)
        {
            await _dbSet.AddRangeAsync(tourGuides);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate)
        {
            try
            {
                var checkDate = tourDate.Date;

                var hasConflict = await _dbContext.Bookings
                    .AnyAsync(b =>
                        b.GuideId.HasValue &&
                        b.GuideId.Value == guideId &&
                        b.TourDate.Date == checkDate &&
                        (b.Status == BookingStatus.Confirmed ||
                         b.Status == BookingStatus.Pending ||
                         b.Status == BookingStatus.Completed));

                return !hasConflict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TourGuideRepository] Error in IsGuideAvailableAsync: {ex.Message}");
                return false;
            }
        }
    }
}
