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
    public class BookingGuestRepository : IBookingGuestRepository
    {
        private readonly DatabaseContext _context;

        public BookingGuestRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<BookingGuest?> GetByIdAsync(int id)
        {
            return await _context.BookingGuests
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
        }

        public async Task<List<BookingGuest>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.BookingGuests
                .Where(g => g.BookingId == bookingId && !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<BookingGuest> CreateAsync(BookingGuest guest)
        {
            guest.CreatedAt = DateTime.UtcNow;
            _context.BookingGuests.Add(guest);
            await _context.SaveChangesAsync();
            return guest;
        }

        public async Task<BookingGuest> UpdateAsync(BookingGuest guest)
        {
            guest.UpdatedAt = DateTime.UtcNow;
            _context.BookingGuests.Update(guest);
            await _context.SaveChangesAsync();
            return guest;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var guest = await GetByIdAsync(id);
            if (guest == null) return false;

            guest.IsDeleted = true;
            guest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByBookingIdAsync(int bookingId)
        {
            var guests = await GetByBookingIdAsync(bookingId);
            if (!guests.Any()) return false;

            foreach (var guest in guests)
            {
                guest.IsDeleted = true;
                guest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
