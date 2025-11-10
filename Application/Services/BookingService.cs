using Application.Interfaces;
using Application.Request.Booking;
using Application.Response.Booking;
using Application.Response.User;
using Domain.Entities.Enums;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingGuestRepository _guestRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITourRepository _tourRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IBookingGuestRepository guestRepository,
            IUserRepository userRepository,
            ITourRepository tourRepository,
            IAuditLogRepository auditLogRepository)
        {
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _userRepository = userRepository;
            _tourRepository = tourRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<BaseResponse<BookingResponse>> GetByIdAsync(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Booking retrieved successfully",
                    Data = booking.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error retrieving booking: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> GetByBookingCodeAsync(string bookingCode)
        {
            try
            {
                var booking = await _bookingRepository.GetByBookingCodeAsync(bookingCode);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // Load related data
                booking = await _bookingRepository.GetByIdWithDetailsAsync(booking.Id);

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Booking retrieved successfully",
                    Data = booking!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error retrieving booking: {ex.Message}"
                };
            }
        }

        public async Task<BookingListResponse> GetAllAsync(int page = 1, int pageSize = 10, string? status = null)
        {
            try
            {
                var query = _bookingRepository.GetAll();

                // Filter by status
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    query = query.Where(b => b.Status == bookingStatus);
                }

                var totalCount = await query.CountAsync();
                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Load guests for each booking
                foreach (var booking in bookings)
                {
                    booking.Guests = await _guestRepository.GetByBookingIdAsync(booking.Id);
                }

                return new BookingListResponse
                {
                    Success = true,
                    Message = "Bookings retrieved successfully",
                    Data = bookings.ToBookingResponseList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new BookingListResponse
                {
                    Success = false,
                    Message = $"Error retrieving bookings: {ex.Message}",
                    Data = new List<BookingResponse>()
                };
            }
        }

        public async Task<BookingListResponse> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var allBookings = await _bookingRepository.GetByUserIdAsync(userId);
                var totalCount = allBookings.Count;

                var bookings = allBookings
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new BookingListResponse
                {
                    Success = true,
                    Message = "User bookings retrieved successfully",
                    Data = bookings.ToBookingResponseList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new BookingListResponse
                {
                    Success = false,
                    Message = $"Error retrieving user bookings: {ex.Message}",
                    Data = new List<BookingResponse>()
                };
            }
        }

        public async Task<BookingListResponse> GetByTourIdAsync(int tourId, int page = 1, int pageSize = 10)
        {
            try
            {
                var allBookings = await _bookingRepository.GetByTourIdAsync(tourId);
                var totalCount = allBookings.Count;

                var bookings = allBookings
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new BookingListResponse
                {
                    Success = true,
                    Message = "Tour bookings retrieved successfully",
                    Data = bookings.ToBookingResponseList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new BookingListResponse
                {
                    Success = false,
                    Message = $"Error retrieving tour bookings: {ex.Message}",
                    Data = new List<BookingResponse>()
                };
            }
        }

        public async Task<BookingListResponse> GetByGuideIdAsync(int guideId, int page = 1, int pageSize = 10)
        {
            try
            {
                var allBookings = await _bookingRepository.GetByGuideIdAsync(guideId);
                var totalCount = allBookings.Count;

                var bookings = allBookings
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new BookingListResponse
                {
                    Success = true,
                    Message = "Guide bookings retrieved successfully",
                    Data = bookings.ToBookingResponseList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new BookingListResponse
                {
                    Success = false,
                    Message = $"Error retrieving guide bookings: {ex.Message}",
                    Data = new List<BookingResponse>()
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> CreateAsync(int userId, CreateBookingRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Validate tour exists and is available
                var tour = await _tourRepository.GetByIdAsync(request.TourId);
                if (tour == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Tour not found"
                    };
                }

                if (tour.Status != TourStatus.Active)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Tour is not available for booking"
                    };
                }

                // Check tour date is in the future
                if (request.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Tour date must be in the future"
                    };
                }

                // Check availability
                var existingGuests = await _bookingRepository.GetTotalBookingsForTourOnDateAsync(request.TourId, request.TourDate);
                if (existingGuests + request.NumberOfGuests > tour.MaxGuests)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = $"Not enough availability. Only {tour.MaxGuests - existingGuests} spots left"
                    };
                }

                // Parse payment method
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid payment method"
                    };
                }

                // Generate unique booking code
                var bookingCode = await GenerateBookingCodeAsync();

                // Calculate total amount
                var totalAmount = tour.Price * request.NumberOfGuests;

                // Create booking
                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    UserId = userId,
                    TourId = request.TourId,
                    TourDate = request.TourDate,
                    NumberOfGuests = request.NumberOfGuests,
                    TotalAmount = totalAmount,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentMethod = paymentMethod,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    SpecialRequests = request.SpecialRequests
                };

                var createdBooking = await _bookingRepository.CreateAsync(booking);

                // Create booking guests
                if (request.Guests != null && request.Guests.Any())
                {
                    foreach (var guestRequest in request.Guests)
                    {
                        var guest = new BookingGuest
                        {
                            BookingId = createdBooking.Id,
                            FullName = guestRequest.FullName,
                            DateOfBirth = guestRequest.DateOfBirth,
                            Gender = guestRequest.Gender,
                            PassportNumber = guestRequest.PassportNumber,
                            Nationality = guestRequest.Nationality,
                            SpecialRequirements = guestRequest.SpecialRequirements
                        };

                        await _guestRepository.CreateAsync(guest);
                    }
                }

                // Load full booking details
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(createdBooking.Id);

                // Log audit
                await LogAuditAsync(userId, "BOOKING_CREATED", "Booking", createdBooking.Id,
                    $"Booking created: {bookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Booking created successfully",
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error creating booking: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> UpdateAsync(int id, UpdateBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // Only allow updates if booking is pending
                if (booking.Status != BookingStatus.Pending)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Can only update pending bookings"
                    };
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.CustomerName))
                    booking.CustomerName = request.CustomerName;

                if (!string.IsNullOrEmpty(request.CustomerEmail))
                    booking.CustomerEmail = request.CustomerEmail;

                if (!string.IsNullOrEmpty(request.CustomerPhone))
                    booking.CustomerPhone = request.CustomerPhone;

                if (request.SpecialRequests != null)
                    booking.SpecialRequests = request.SpecialRequests;

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                // Update guests if provided
                if (request.Guests != null && request.Guests.Any())
                {
                    // Delete existing guests
                    await _guestRepository.DeleteByBookingIdAsync(id);

                    // Create new guests
                    foreach (var guestRequest in request.Guests)
                    {
                        var guest = new BookingGuest
                        {
                            BookingId = id,
                            FullName = guestRequest.FullName,
                            DateOfBirth = guestRequest.DateOfBirth,
                            Gender = guestRequest.Gender,
                            PassportNumber = guestRequest.PassportNumber,
                            Nationality = guestRequest.Nationality,
                            SpecialRequirements = guestRequest.SpecialRequirements
                        };

                        await _guestRepository.CreateAsync(guest);
                    }
                }

                // Load full booking details
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                // Log audit
                await LogAuditAsync(booking.UserId, "BOOKING_UPDATED", "Booking", id,
                    $"Booking updated: {booking.BookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Booking updated successfully",
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error updating booking: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> UpdateStatusAsync(int id, UpdateBookingStatusRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // Parse status
                if (!Enum.TryParse<BookingStatus>(request.Status, true, out var bookingStatus))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid booking status"
                    };
                }

                booking.Status = bookingStatus;
                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                // Load full booking details
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                // Log audit
                await LogAuditAsync(booking.UserId, "BOOKING_STATUS_UPDATED", "Booking", id,
                    $"Booking status changed to {bookingStatus}: {booking.BookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Booking status updated successfully",
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error updating booking status: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> UpdatePaymentAsync(int id, UpdatePaymentRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // Parse payment status
                if (!Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var paymentStatus))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid payment status"
                    };
                }

                booking.PaymentStatus = paymentStatus;
                booking.PaymentTransactionId = request.PaymentTransactionId;
                booking.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;

                if (request.RefundAmount.HasValue)
                {
                    booking.RefundAmount = request.RefundAmount.Value;
                }

                // If payment is completed, confirm the booking
                if (paymentStatus == PaymentStatus.Paid && booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Confirmed;
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                // Load full booking details
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                // Log audit
                await LogAuditAsync(booking.UserId, "BOOKING_PAYMENT_UPDATED", "Booking", id,
                    $"Payment status changed to {paymentStatus}: {booking.BookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = "Payment updated successfully",
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error updating payment: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse<BookingResponse>> CancelAsync(int id, CancelBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // Check if booking can be cancelled
                if (booking.Status == BookingStatus.Cancelled)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking is already cancelled"
                    };
                }

                if (booking.Status == BookingStatus.Completed)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Cannot cancel completed booking"
                    };
                }

                // Calculate refund amount (example: 100% if cancelled 7+ days before, 50% if 3-7 days, 0% if < 3 days)
                var daysUntilTour = (booking.TourDate.Date - DateTime.UtcNow.Date).Days;
                decimal refundPercentage = 0;

                if (daysUntilTour >= 7)
                    refundPercentage = 1.0m; // 100%
                else if (daysUntilTour >= 3)
                    refundPercentage = 0.5m; // 50%
                else
                    refundPercentage = 0m; // 0%

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = request.CancellationReason;
                booking.RefundAmount = booking.TotalAmount * refundPercentage;

                if (booking.PaymentStatus == PaymentStatus.Paid && booking.RefundAmount > 0)
                {
                    booking.PaymentStatus = PaymentStatus.Refunded;
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                // Load full booking details
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                // Log audit
                await LogAuditAsync(booking.UserId, "BOOKING_CANCELLED", "Booking", id,
                    $"Booking cancelled: {booking.BookingCode}, Refund: {booking.RefundAmount}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = $"Booking cancelled successfully. Refund amount: {booking.RefundAmount:C}",
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error cancelling booking: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> DeleteAsync(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);

                if (booking == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                var result = await _bookingRepository.DeleteAsync(id);

                if (!result)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Failed to delete booking"
                    };
                }

                // Log audit
                await LogAuditAsync(booking.UserId, "BOOKING_DELETED", "Booking", id,
                    $"Booking deleted: {booking.BookingCode}");

                return new BaseResponse
                {
                    Success = true,
                    Message = "Booking deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Error deleting booking: {ex.Message}"
                };
            }
        }

        #region Private Helper Methods

        private async Task<string> GenerateBookingCodeAsync()
        {
            string code;
            bool exists;

            do
            {
                // Format: BK-YYYYMMDD-XXXX (e.g., BK-20241210-A1B2)
                var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
                var randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
                code = $"BK-{datePart}-{randomPart}";

                exists = await _bookingRepository.BookingCodeExistsAsync(code);
            }
            while (exists);

            return code;
        }

        private async Task LogAuditAsync(int? userId, string action, string entityName,
            int entityId, string? newValues = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    NewValues = newValues
                };

                await _auditLogRepository.CreateAsync(auditLog);
            }
            catch
            {
                // Log audit failure silently
            }
        }

        #endregion
    }
}
