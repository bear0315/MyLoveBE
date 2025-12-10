using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class TourDepartureRepository : ITourDepartureRepository
    {
        private readonly DatabaseContext _context;

        public TourDepartureRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<TourDeparture?> GetByIdAsync(int id)
        {
            return await _context.TourDepartures
                .Include(td => td.Tour)
                .Include(td => td.DefaultGuide)
                .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);
        }

        public async Task<TourDeparture?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.TourDepartures
                .Include(td => td.Tour)
                    .ThenInclude(t => t.Destination)
                .Include(td => td.DefaultGuide)
                    .ThenInclude(g => g.User)
                .Include(td => td.Bookings.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);
        }

        public async Task<List<TourDeparture>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourDepartures
                .Include(td => td.DefaultGuide)
                .Where(td => td.TourId == tourId && !td.IsDeleted)
                .OrderBy(td => td.DepartureDate)
                .ToListAsync();
        }

        public async Task<List<TourDeparture>> GetAvailableDeparturesAsync(int tourId, DateTime? fromDate = null)
        {
            var query = _context.TourDepartures
                .Include(td => td.DefaultGuide)
                .Where(td => td.TourId == tourId
                    && !td.IsDeleted
                    && td.Status == DepartureStatus.Available
                    && td.DepartureDate >= (fromDate ?? DateTime.UtcNow.Date));

            return await query
                .OrderBy(td => td.DepartureDate)
                .ToListAsync();
        }

        public async Task<TourDeparture?> GetByTourAndDateAsync(int tourId, DateTime date)
        {
            return await _context.TourDepartures
                .Include(td => td.Tour)
                .Include(td => td.DefaultGuide)
                .FirstOrDefaultAsync(td => td.TourId == tourId
                    && td.DepartureDate.Date == date.Date
                    && !td.IsDeleted);
        }

        public async Task<TourDeparture> CreateAsync(TourDeparture departure)
        {
            departure.CreatedAt = DateTime.UtcNow;
            _context.TourDepartures.Add(departure);
            await _context.SaveChangesAsync();
            return departure;
        }

        public async Task<TourDeparture> UpdateAsync(TourDeparture departure)
        {
            departure.UpdatedAt = DateTime.UtcNow;
            _context.TourDepartures.Update(departure);
            await _context.SaveChangesAsync();
            return departure;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var departure = await GetByIdAsync(id);
            if (departure == null) return false;

            departure.IsDeleted = true;
            departure.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckAvailabilityAsync(int departureId, int numberOfGuests)
        {
            var departure = await GetByIdAsync(departureId);
            if (departure == null) return false;

            return departure.AvailableSlots >= numberOfGuests;
        }

        public async Task UpdateBookedGuestsAsync(int departureId)
        {
            var departure = await GetByIdAsync(departureId);
            if (departure == null) return;

            // Tính tổng số khách đã đặt (trừ cancelled)
            departure.BookedGuests = await _context.Bookings
                .Where(b => b.TourDepartureId == departureId
                    && b.Status != BookingStatus.Cancelled
                    && !b.IsDeleted)
                .SumAsync(b => b.NumberOfGuests);

            // Cập nhật status
            if (departure.BookedGuests >= departure.MaxGuests)
            {
                departure.Status = DepartureStatus.Full;
            }
            else if (departure.BookedGuests >= departure.MaxGuests * 0.8m)
            {
                departure.Status = DepartureStatus.AlmostFull;
            }
            else
            {
                departure.Status = DepartureStatus.Available;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<TourDeparture>> BulkCreateAsync(List<TourDeparture> departures)
        {
            var now = DateTime.UtcNow;
            foreach (var departure in departures)
            {
                departure.CreatedAt = now;
            }

            _context.TourDepartures.AddRange(departures);
            await _context.SaveChangesAsync();
            return departures;
        }
    }
}
