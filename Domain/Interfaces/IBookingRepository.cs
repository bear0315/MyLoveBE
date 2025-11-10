using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetByBookingCodeAsync(string bookingCode);
        Task<Booking?> GetByIdWithDetailsAsync(int id);
        IQueryable<Booking> GetAll();
        Task<List<Booking>> GetByUserIdAsync(int userId);
        Task<List<Booking>> GetByTourIdAsync(int tourId);
        Task<List<Booking>> GetByGuideIdAsync(int guideId);
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> BookingCodeExistsAsync(string bookingCode);
        Task<int> GetTotalBookingsForTourOnDateAsync(int tourId, DateTime tourDate);
    }
}
