using Application.Response.Booking;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings
{
    public static class BookingMapper
    {
        public static BookingResponse ToBookingResponse(this Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            return new BookingResponse
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,
                UserId = booking.UserId,
                UserName = booking.User?.FullName ?? string.Empty,

                TourId = booking.TourId,
                TourName = booking.Tour?.Name ?? string.Empty,
                TourLocation = booking.Tour?.Location ?? string.Empty,

                GuideId = booking.GuideId,
                GuideName = booking.Guide?.FullName ?? string.Empty,
                GuideEmail= booking.Guide?.Email ?? string.Empty,
                GuidePhone =booking.Guide?.PhoneNumber ?? string.Empty,

                TourDate = booking.TourDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),

                // Payment
                PaymentStatus = booking.PaymentStatus.ToString(),
                PaymentMethod = booking.PaymentMethod.ToString(),
                PaymentTransactionId = booking.PaymentTransactionId,
                PaymentDate = booking.PaymentDate,

                // Customer info
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CustomerPhone = booking.CustomerPhone,
                SpecialRequests = booking.SpecialRequests,

                // Cancellation
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                RefundAmount = booking.RefundAmount,

                // Guests
                Guests = booking.Guests?
                    .Select(g => g.ToBookingGuestResponse())
                    .ToList()
                    ?? new List<BookingGuestResponse>(),

                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }

        public static BookingGuestResponse ToBookingGuestResponse(this BookingGuest guest)
        {
            if (guest == null)
                throw new ArgumentNullException(nameof(guest));

            return new BookingGuestResponse
            {
                Id = guest.Id,
                FullName = guest.FullName,
                DateOfBirth = guest.DateOfBirth,
                Gender = guest.Gender,
                PassportNumber = guest.PassportNumber,
                Nationality = guest.Nationality,
                SpecialRequirements = guest.SpecialRequirements,
            };
        }

        public static List<BookingResponse> ToBookingResponseList(this IEnumerable<Booking> bookings)
        {
            return bookings.Select(b => b.ToBookingResponse()).ToList();
        }
    }
}
