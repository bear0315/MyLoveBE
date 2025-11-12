using Domain.Entities;
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
        public TourGuideRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourGuide>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourGuides
                .Include(tg => tg.Guide)          
                    .ThenInclude(g => g.User)   
                .Include(tg => tg.Tour)           
                .Where(tg => tg.TourId == tourId)
                .ToListAsync();
        }
        public async Task<IEnumerable<TourGuide>> GetByGuideIdAsync(int guideId)
        {
            return await _context.TourGuides
                .Include(tg => tg.Tour)
                .Where(tg => tg.GuideId == guideId)
                .ToListAsync();
        }

        public async Task<TourGuide?> GetDefaultGuideForTourAsync(int tourId)
        {
            return await _context.TourGuides
                .Include(tg => tg.Guide)
                    .ThenInclude(g => g.User)
                .FirstOrDefaultAsync(tg => tg.TourId == tourId && tg.IsDefault);
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var tourGuides = await _context.TourGuides
                .Where(tg => tg.TourId == tourId)
                .ToListAsync();

            _context.TourGuides.RemoveRange(tourGuides);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(List<TourGuide> tourGuides)
        {
            await _context.TourGuides.AddRangeAsync(tourGuides);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsGuideAvailableAsync(int guideId, DateTime tourDate)
        {
            return !await _context.Bookings
                .AnyAsync(b => b.GuideId == guideId &&
                             b.TourDate.Date == tourDate.Date &&
                             b.Status != Domain.Entities.Enums.BookingStatus.Cancelled);
        }
    }
}
