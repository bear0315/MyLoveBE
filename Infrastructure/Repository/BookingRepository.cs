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
    public class BookingRepository : IBookingRepository
    {
        private readonly DatabaseContext _context;

        public BookingRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<Booking?> GetByBookingCodeAsync(string bookingCode)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode && !b.IsDeleted);
        }

        public async Task<Booking?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                    .ThenInclude(t => t.Destination)
                .Include(b => b.Guide)
                .Include(b => b.Guests)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public IQueryable<Booking> GetAll()
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Guide)
                .Where(b => !b.IsDeleted)
                .AsQueryable();
        }

        public async Task<List<Booking>> GetByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                    .ThenInclude(t => t.Destination)
                .Include(b => b.Guide)
                .Include(b => b.Guests)
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByTourIdAsync(int tourId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Guide)
                .Include(b => b.Guests)
                .Where(b => b.TourId == tourId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByGuideIdAsync(int guideId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Guests)
                .Where(b => b.GuideId == guideId && !b.IsDeleted)
                .OrderByDescending(b => b.TourDate)
                .ToListAsync();
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            booking.CreatedAt = DateTime.UtcNow;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await GetByIdAsync(id);
            if (booking == null) return false;

            booking.IsDeleted = true;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Bookings.AnyAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<bool> BookingCodeExistsAsync(string bookingCode)
        {
            return await _context.Bookings.AnyAsync(b => b.BookingCode == bookingCode && !b.IsDeleted);
        }

        public async Task<int> GetTotalBookingsForTourOnDateAsync(int tourId, DateTime tourDate)
        {
            return await _context.Bookings
                .Where(b => b.TourId == tourId
                    && b.TourDate.Date == tourDate.Date
                    && b.Status != BookingStatus.Cancelled
                    && !b.IsDeleted)
                .SumAsync(b => b.NumberOfGuests);
        }
    }
}
