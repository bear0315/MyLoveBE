using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBookingGuestRepository
    {
        Task<BookingGuest?> GetByIdAsync(int id);
        Task<List<BookingGuest>> GetByBookingIdAsync(int bookingId);
        Task<BookingGuest> CreateAsync(BookingGuest guest);
        Task<BookingGuest> UpdateAsync(BookingGuest guest);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByBookingIdAsync(int bookingId);
    }
}
