using Application.Request.Booking;
using Application.Response.Booking;
using Application.Response.Guide;
using Application.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookingService
    {
        Task<BaseResponse<BookingResponse>> GetByIdAsync(int id);
        Task<BaseResponse<BookingResponse>> GetByBookingCodeAsync(string bookingCode);
        Task<BookingListResponse> GetAllAsync(int page = 1, int pageSize = 10, string? status = null);
        Task<BookingListResponse> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
        Task<BookingListResponse> GetByTourIdAsync(int tourId, int page = 1, int pageSize = 10);
        Task<BookingListResponse> GetByGuideIdAsync(int guideId, int page = 1, int pageSize = 10);
        Task<BaseResponse<BookingResponse>> CreateAsync(int userId, CreateBookingRequest request);
        Task<BaseResponse<BookingResponse>> UpdateAsync(int id, UpdateBookingRequest request);
        Task<BaseResponse<BookingResponse>> UpdateStatusAsync(int id, UpdateBookingStatusRequest request);
        Task<BaseResponse<BookingResponse>> UpdatePaymentAsync(int id, UpdatePaymentRequest request);
        Task<BaseResponse<BookingResponse>> CancelAsync(int id, CancelBookingRequest request);
        Task<GuideAvailabilityListResponse> GetAvailableGuidesForBookingAsync(int tourId, DateTime tourDate);
        Task<BaseResponse> DeleteAsync(int id);
    }
}
