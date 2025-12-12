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
using Application.Response.Guide;
using Application.Response.Booking.Application.Response.Booking;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingGuestRepository _guestRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITourRepository _tourRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITourGuideRepository _tourGuideRepository;  
        private readonly ITourDepartureRepository _tourDepartureRepository;
        private readonly ITourService _tourService;
        private readonly ILoyaltyService _loyaltyService;


        public BookingService(
           IBookingRepository bookingRepository,
           IBookingGuestRepository guestRepository,
           IUserRepository userRepository,
           ITourRepository tourRepository,
           IAuditLogRepository auditLogRepository,
           ITourGuideRepository tourGuideRepository,  
           ITourService tourService, ITourDepartureRepository _tourDepartureRepository, ILoyaltyService loyaltyService)  
        {
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _userRepository = userRepository;
            _tourRepository = tourRepository;
            _auditLogRepository = auditLogRepository;
            _tourGuideRepository = tourGuideRepository;  
            _tourService = tourService;
            this._tourDepartureRepository = _tourDepartureRepository;
            _loyaltyService = loyaltyService;

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
                // ==================== VALIDATE USER ====================
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }

                // ==================== VALIDATE TOUR ====================
                var tour = await _tourRepository.GetByIdAsync(request.TourId);
                if (tour == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy tour"
                    };
                }

                if (tour.Status != TourStatus.Active)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Tour hiện không khả dụng để đặt"
                    };
                }

                // ==================== VALIDATE TOUR DATE ====================
                if (request.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Ngày tour phải trong tương lai"
                    };
                }

                // ==================== XỬ LÝ TOUR DEPARTURE ====================
                TourDeparture? tourDeparture = null;
                int? selectedDepartureId = null;

                // Option 1: User chọn departure cụ thể
                if (request.TourDepartureId.HasValue)
                {
                    tourDeparture = await _tourDepartureRepository.GetByIdAsync(request.TourDepartureId.Value);

                    if (tourDeparture == null)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = "Không tìm thấy chuyến khởi hành"
                        };
                    }

                    if (tourDeparture.TourId != request.TourId)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = "Chuyến khởi hành không thuộc tour này"
                        };
                    }

                    if (tourDeparture.Status == DepartureStatus.Full ||
                        tourDeparture.Status == DepartureStatus.Cancelled)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Chuyến khởi hành đã {(tourDeparture.Status == DepartureStatus.Full ? "đầy chỗ" : "bị hủy")}"
                        };
                    }

                    if (tourDeparture.AvailableSlots < request.NumberOfGuests)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Không đủ chỗ. Chỉ còn {tourDeparture.AvailableSlots} chỗ"
                        };
                    }

                    selectedDepartureId = tourDeparture.Id;
                }
                // Option 2: Tự động tìm departure theo TourDate
                else
                {
                    tourDeparture = await _tourDepartureRepository.GetByTourAndDateAsync(
                        request.TourId,
                        request.TourDate);

                    if (tourDeparture != null)
                    {
                        if (tourDeparture.Status == DepartureStatus.Full ||
                            tourDeparture.Status == DepartureStatus.Cancelled)
                        {
                            return new BaseResponse<BookingResponse>
                            {
                                Success = false,
                                Message = $"Không có chuyến khởi hành vào ngày {request.TourDate:dd/MM/yyyy}"
                            };
                        }

                        if (tourDeparture.AvailableSlots < request.NumberOfGuests)
                        {
                            return new BaseResponse<BookingResponse>
                            {
                                Success = false,
                                Message = $"Không đủ chỗ vào ngày {request.TourDate:dd/MM/yyyy}. Chỉ còn {tourDeparture.AvailableSlots} chỗ"
                            };
                        }

                        selectedDepartureId = tourDeparture.Id;
                    }
                    else
                    {
                        // Fallback: Kiểm tra theo cách cũ
                        var existingGuests = await _bookingRepository
                            .GetTotalBookingsForTourOnDateAsync(request.TourId, request.TourDate);

                        if (existingGuests + request.NumberOfGuests > tour.MaxGuests)
                        {
                            return new BaseResponse<BookingResponse>
                            {
                                Success = false,
                                Message = $"Không đủ chỗ. Chỉ còn {tour.MaxGuests - existingGuests} chỗ"
                            };
                        }
                    }
                }

                // ==================== VALIDATE PAYMENT METHOD ====================
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Phương thức thanh toán không hợp lệ"
                    };
                }

                // ==================== TÍNH GIÁ GỐC ====================
                var effectivePrice = tourDeparture?.SpecialPrice ?? tour.Price;
                var originalAmount = effectivePrice * request.NumberOfGuests; // VND

                // ==================== GIẢM GIÁ HẠNG THÀNH VIÊN ====================
                var memberDiscount = _loyaltyService.CalculateDiscount(originalAmount, user.MemberTier);
                var amountAfterMemberDiscount = originalAmount - memberDiscount;

                // ==================== XỬ LÝ ĐỔI ĐIỂM ====================
                decimal pointsDiscount = 0;
                int pointsRedeemed = 0;

                if (request.PointsToRedeem.HasValue && request.PointsToRedeem.Value > 0)
                {
                    // Validate 1: Điểm phải là bội số của 100
                    if (request.PointsToRedeem.Value % 100 != 0)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = "Số điểm đổi phải là bội số của 100 (100 điểm = 1,000 VND)"
                        };
                    }

                    // Validate 2: Điểm có đủ không
                    if (user.LoyaltyPoints < request.PointsToRedeem.Value)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Không đủ điểm. Bạn có {user.LoyaltyPoints:N0} điểm"
                        };
                    }

                    // Validate 3: Không vượt quá 50% giá trị booking
                    var maxPoints = _loyaltyService.CalculateMaxRedeemablePoints(amountAfterMemberDiscount);
                    if (request.PointsToRedeem.Value > maxPoints)
                    {
                        var maxValue = maxPoints * 10; // 100 points = 1,000 VND
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Tối đa {maxPoints:N0} điểm (50% giá trị booking = {maxValue:N0} VND)"
                        };
                    }

                    // Chuyển đổi điểm thành tiền: 100 points = 1,000 VND
                    pointsDiscount = await _loyaltyService.ConvertPointsToMoneyAsync(
                        userId,
                        request.PointsToRedeem.Value);

                    pointsRedeemed = request.PointsToRedeem.Value;
                }

                // ==================== TÍNH TỔNG GIÁ CUỐI CÙNG ====================
                var totalAmount = amountAfterMemberDiscount - pointsDiscount;

                // ==================== XỬ LÝ GUIDE SELECTION ====================
                int? selectedGuideId = null;

                // Priority 1: Default guide của departure (nếu có)
                if (tourDeparture?.DefaultGuideId != null)
                {
                    var isGuideAvailable = await _tourService.IsGuideAvailableAsync(
                        tourDeparture.DefaultGuideId.Value,
                        request.TourDate);

                    if (isGuideAvailable)
                    {
                        selectedGuideId = tourDeparture.DefaultGuideId.Value;
                    }
                }

                // Priority 2: Guide do khách chọn
                if (!selectedGuideId.HasValue && request.GuideId.HasValue)
                {
                    var availableGuides = await _tourService.GetAvailableGuidesForTourAsync(
                        request.TourId,
                        request.TourDate);

                    var selectedGuide = availableGuides.FirstOrDefault(g => g.GuideId == request.GuideId.Value);

                    if (selectedGuide == null)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Hướng dẫn viên ID {request.GuideId.Value} không được phân công cho tour này"
                        };
                    }

                    if (!selectedGuide.IsAvailable)
                    {
                        return new BaseResponse<BookingResponse>
                        {
                            Success = false,
                            Message = $"Hướng dẫn viên {selectedGuide.FullName} không khả dụng vào ngày {request.TourDate:dd/MM/yyyy}"
                        };
                    }

                    selectedGuideId = request.GuideId.Value;
                }
                // Priority 3: Default guide của tour
                else if (!selectedGuideId.HasValue)
                {
                    var defaultGuideId = await _tourService.GetDefaultGuideIdForTourAsync(
                        request.TourId,
                        request.TourDate);

                    if (defaultGuideId.HasValue)
                    {
                        selectedGuideId = defaultGuideId.Value;
                    }
                }

                // ==================== TẠO BOOKING CODE ====================
                var bookingCode = await GenerateBookingCodeAsync();

                // ==================== TẠO BOOKING ====================
                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    UserId = userId,
                    TourId = request.TourId,
                    TourDepartureId = selectedDepartureId,
                    GuideId = selectedGuideId,
                    TourDate = request.TourDate,
                    NumberOfGuests = request.NumberOfGuests,
                    TotalAmount = totalAmount,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentMethod = paymentMethod,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    SpecialRequests = request.SpecialRequests,
                    PointsRedeemed = pointsRedeemed,
                    PointsDiscount = pointsDiscount
                };

                var createdBooking = await _bookingRepository.CreateAsync(booking);

                // ==================== TẠO BOOKING GUESTS ====================
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

                // ==================== LOAD BOOKING WITH DETAILS ====================
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(createdBooking.Id);

                // ==================== TẠO MESSAGE RESPONSE ====================
                var message = "Đặt tour thành công! ";

                if (memberDiscount > 0)
                {
                    message += $"Giảm giá thành viên {user.MemberTier}: {memberDiscount:N0} VND. ";
                }

                if (pointsDiscount > 0)
                {
                    message += $"Giảm giá điểm thưởng: {pointsDiscount:N0} VND ({pointsRedeemed:N0} điểm đã sử dụng). ";
                }

                message += $"Tổng thanh toán: {totalAmount:N0} VND";

                // ==================== LOG AUDIT ====================
                var departureInfo = selectedDepartureId.HasValue
                    ? $", Departure ID: {selectedDepartureId.Value}"
                    : "";

                var guideInfo = selectedGuideId.HasValue
                    ? $", Guide ID: {selectedGuideId.Value}"
                    : ", No guide assigned";

                var pointsInfo = pointsRedeemed > 0
                    ? $", Points redeemed: {pointsRedeemed}"
                    : "";

                await LogAuditAsync(userId, "BOOKING_CREATED", "Booking", createdBooking.Id,
                    $"Booking created: {bookingCode}{departureInfo}{guideInfo}{pointsInfo}");

                // ==================== RETURN RESPONSE ====================
                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = message,
                    Data = bookingWithDetails!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Lỗi khi tạo booking: {ex.Message}"
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

                if (!Enum.TryParse<BookingStatus>(request.Status, true, out var newBookingStatus))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid booking status"
                    };
                }

                var oldStatus = booking.Status;

                // === CỘNG ĐIỂM KHI TOUR HOÀN THÀNH (Completed) ===
                if (newBookingStatus == BookingStatus.Completed && oldStatus != BookingStatus.Completed)
                {
                    // Chỉ cộng điểm nếu đã thanh toán (Paid hoặc PartiallyPaid)
                    if (booking.PaymentStatus == PaymentStatus.Paid ||
                        booking.PaymentStatus == PaymentStatus.PartiallyPaid)
                    {
                        var pointsEarned = await _loyaltyService.AddPointsAsync(
                            booking.UserId,
                            booking.TotalAmount,
                            $"Tour completed - Booking {booking.BookingCode}");

                        // Ghi log để biết được cộng bao nhiêu điểm
                        await LogAuditAsync(booking.UserId, "LOYALTY_POINTS_EARNED", "Booking", id,
                            $"Earned {pointsEarned} points for completed tour. Booking: {booking.BookingCode}");
                    }
                }

                // === XỬ LÝ SLOT (giữ nguyên logic cũ của bạn) ===
                if (oldStatus == BookingStatus.Pending && newBookingStatus == BookingStatus.Confirmed)
                {
                    if (booking.PaymentStatus == PaymentStatus.Paid ||
                        booking.PaymentStatus == PaymentStatus.PartiallyPaid)
                    {
                        if (booking.TourDepartureId.HasValue)
                        {
                            await _tourDepartureRepository.UpdateBookedGuestsAsync(booking.TourDepartureId.Value);
                        }
                    }
                }

                if (newBookingStatus == BookingStatus.Cancelled || newBookingStatus == BookingStatus.NoShow)
                {
                    if ((oldStatus == BookingStatus.Confirmed || oldStatus == BookingStatus.Completed) &&
                        (booking.PaymentStatus == PaymentStatus.Paid ||
                         booking.PaymentStatus == PaymentStatus.PartiallyPaid ||
                         booking.PaymentStatus == PaymentStatus.Refunded))
                    {
                        if (booking.TourDepartureId.HasValue)
                        {
                            await _tourDepartureRepository.UpdateBookedGuestsAsync(booking.TourDepartureId.Value);
                        }
                    }
                }

                // Cập nhật trạng thái booking
                booking.Status = newBookingStatus;
                await _bookingRepository.UpdateAsync(booking);

                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                await LogAuditAsync(booking.UserId, "BOOKING_STATUS_UPDATED", "Booking", id,
                    $"Status changed from {oldStatus} → {newBookingStatus}: {booking.BookingCode}");

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

                var oldPaymentStatus = booking.PaymentStatus;
                var oldBookingStatus = booking.Status;

                if (!Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var newPaymentStatus))
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Invalid payment status"
                    };
                }

                booking.PaymentStatus = newPaymentStatus;
                booking.PaymentTransactionId = request.PaymentTransactionId;
                booking.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;

                if (request.RefundAmount.HasValue)
                {
                    booking.RefundAmount = request.RefundAmount.Value;
                }

                // ========== CỘNG ĐIỂM KHI THANH TOÁN THÀNH CÔNG ==========
                var pointsEarned = 0;
                if ((newPaymentStatus == PaymentStatus.Paid || newPaymentStatus == PaymentStatus.PartiallyPaid) &&
                    (oldPaymentStatus != PaymentStatus.Paid && oldPaymentStatus != PaymentStatus.PartiallyPaid))
                {
                    // Tự động xác nhận booking
                    if (booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Confirmed;
                    }

                    // Trừ slot
                    if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Completed)
                    {
                        if (booking.TourDepartureId.HasValue)
                        {
                            await _tourDepartureRepository.UpdateBookedGuestsAsync(booking.TourDepartureId.Value);
                        }
                    }

                    // CỘNG ĐIỂM THƯỞNG
                    pointsEarned = await _loyaltyService.AddPointsAsync(
                        booking.UserId,
                        booking.TotalAmount,
                        $"Booking {booking.BookingCode}");
                }

                // ========== TRỪ ĐIỂM NÊU REFUND ==========
                if ((newPaymentStatus == PaymentStatus.Refunded || newPaymentStatus == PaymentStatus.Failed) &&
                    (oldPaymentStatus == PaymentStatus.Paid || oldPaymentStatus == PaymentStatus.PartiallyPaid))
                {
                    if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Completed)
                    {
                        if (booking.TourDepartureId.HasValue)
                        {
                            await _tourDepartureRepository.UpdateBookedGuestsAsync(booking.TourDepartureId.Value);
                        }
                    }

                    // TRỪ ĐIỂM ĐÃ CỘNG (nếu cần)
                    var pointsToDeduct = _loyaltyService.CalculatePointsEarned(booking.TotalAmount);
                    await _loyaltyService.RedeemPointsAsync(booking.UserId, pointsToDeduct);
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);
                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                var message = "Payment updated successfully";
                if (pointsEarned > 0)
                {
                    message += $". You earned {pointsEarned} loyalty points!";
                }

                await LogAuditAsync(booking.UserId, "BOOKING_PAYMENT_UPDATED", "Booking", id,
                    $"Payment status changed from {oldPaymentStatus} to {newPaymentStatus}: {booking.BookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = message,
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

                var oldStatus = booking.Status;
                var oldPaymentStatus = booking.PaymentStatus;

                // Tính tiền hoàn lại
                var daysUntilTour = (booking.TourDate.Date - DateTime.UtcNow.Date).Days;
                decimal refundPercentage = daysUntilTour >= 7 ? 1.0m :
                                          daysUntilTour >= 3 ? 0.5m : 0m;

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = request.CancellationReason;
                booking.RefundAmount = booking.TotalAmount * refundPercentage;

                // Cập nhật payment status nếu đã thanh toán
                if (booking.PaymentStatus == PaymentStatus.Paid && booking.RefundAmount > 0)
                {
                    booking.PaymentStatus = PaymentStatus.Refunded;
                }

                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                // === CỘNG LẠI SLOT KHI HỦY ===
                // CHỈ cộng lại nếu:
                // 1. Booking đã Confirmed hoặc Completed (đã trừ slot)
                // 2. VÀ đã thanh toán (Paid/PartiallyPaid)
                if ((oldStatus == BookingStatus.Confirmed || oldStatus == BookingStatus.Completed) &&
                    (oldPaymentStatus == PaymentStatus.Paid || oldPaymentStatus == PaymentStatus.PartiallyPaid))
                {
                    if (booking.TourDepartureId.HasValue)
                    {
                        await _tourDepartureRepository.UpdateBookedGuestsAsync(booking.TourDepartureId.Value);
                    }
                }
                // Nếu booking chỉ Pending + chưa Paid → không cần cộng lại vì chưa trừ

                var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(id);

                await LogAuditAsync(booking.UserId, "BOOKING_CANCELLED", "Booking", id,
                    $"Booking cancelled: {booking.BookingCode}, Refund: {booking.RefundAmount:C}");

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

        public async Task<GuideAvailabilityListResponse> GetAvailableGuidesForBookingAsync(int tourId, DateTime tourDate)
        {
            try
            {
                // Validate tour exists
                var tour = await _tourRepository.GetByIdAsync(tourId);
                if (tour == null)
                {
                    return new GuideAvailabilityListResponse
                    {
                        Success = false,
                        Message = "Tour not found"
                    };
                }

                // Get available guides from TourService
                var availableGuides = await _tourService.GetAvailableGuidesForTourAsync(tourId, tourDate);

                if (!availableGuides.Any())
                {
                    return new GuideAvailabilityListResponse
                    {
                        Success = true,
                        Message = "No guides available for this tour on the selected date",
                        Data = new List<GuideAvailabilityResponse>()
                    };
                }

                // Map to response
                var guideResponses = new List<GuideAvailabilityResponse>();

                foreach (var guide in availableGuides)
                {
                    var response = new GuideAvailabilityResponse
                    {
                        GuideId = guide.GuideId,
                        FullName = guide.FullName,
                        Avatar = guide.Avatar,
                        IsAvailable = guide.IsAvailable,
                    };

                    guideResponses.Add(response);
                }

                // Sort: available first, then by default guide, then by name
                var sortedGuides = guideResponses
                    .OrderByDescending(g => g.IsAvailable)
                    .ThenByDescending(g => g.IsDefaultGuide)
                    .ThenBy(g => g.FullName)
                    .ToList();

                return new GuideAvailabilityListResponse
                {
                    Success = true,
                    Message = $"Found {sortedGuides.Count(g => g.IsAvailable)} available guide(s) for this tour",
                    Data = sortedGuides
                };
            }
            catch (Exception ex)
            {
                return new GuideAvailabilityListResponse
                {
                    Success = false,
                    Message = $"Error retrieving available guides: {ex.Message}",
                    Data = new List<GuideAvailabilityResponse>()
                };
            }
        }
        public async Task<BaseResponse<BookingResponse>> AssignGuideAsync(int bookingId, int? guideId)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);

                if (booking == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                if (booking.Status == BookingStatus.Cancelled ||
                    booking.Status == BookingStatus.Completed)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = $"Cannot assign guide to {booking.Status} booking"
                    };
                }

                if (!guideId.HasValue)
                {
                    booking.GuideId = null;
                    await _bookingRepository.UpdateAsync(booking);

                    var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

                    await LogAuditAsync(null, "GUIDE_REMOVED", "Booking", bookingId,
                        $"Guide removed from booking: {booking.BookingCode}");

                    return new BaseResponse<BookingResponse>
                    {
                        Success = true,
                        Message = "Guide removed from booking successfully",
                        Data = bookingWithDetails!.ToBookingResponse()
                    };
                }

              
                var availableGuides = await _tourService.GetAvailableGuidesForTourAsync(
                    booking.TourId,
                    booking.TourDate);

                var selectedGuide = availableGuides.FirstOrDefault(g => g.GuideId == guideId.Value);

                if (selectedGuide == null)
                {
                    return new BaseResponse<BookingResponse>
                    {
                        Success = false,
                        Message = $"Guide with ID {guideId.Value} is not assigned to this tour"
                    };
                }

                string warningMessage = "";
                if (!selectedGuide.IsAvailable)
                {
                    warningMessage = $" Warning: Guide {selectedGuide.FullName} is not available on {booking.TourDate:yyyy-MM-dd}";
                }

                booking.GuideId = guideId.Value;
                await _bookingRepository.UpdateAsync(booking);

                var updatedBooking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

                await LogAuditAsync(null, "GUIDE_ASSIGNED", "Booking", bookingId,
                    $"Guide {guideId.Value} assigned to booking: {booking.BookingCode}");

                return new BaseResponse<BookingResponse>
                {
                    Success = true,
                    Message = $"Guide assigned successfully.{warningMessage}",
                    Data = updatedBooking!.ToBookingResponse()
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BookingResponse>
                {
                    Success = false,
                    Message = $"Error assigning guide: {ex.Message}"
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
