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
    public class TourItineraryRepository : GenericRepository<TourItinerary>, ITourItineraryRepository
    {
        public TourItineraryRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<TourItinerary>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourItineraries
                .Where(i => i.TourId == tourId)
                .OrderBy(i => i.DayNumber)
                .ToListAsync();
        }

        public async Task<TourItinerary?> GetByTourAndDayAsync(int tourId, int dayNumber)
        {
            return await _context.TourItineraries
                .FirstOrDefaultAsync(i => i.TourId == tourId && i.DayNumber == dayNumber);
        }

        public async Task BulkDeleteByTourIdAsync(int tourId)
        {
            var itineraries = await _context.TourItineraries
                .Where(i => i.TourId == tourId)
                .ToListAsync();

            _context.TourItineraries.RemoveRange(itineraries);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(List<TourItinerary> itineraries)
        {
            await _context.TourItineraries.AddRangeAsync(itineraries);
            await _context.SaveChangesAsync();
        }
    }
}
