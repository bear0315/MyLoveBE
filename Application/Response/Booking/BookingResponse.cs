using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Booking
{
    using System;
    using System.Collections.Generic;

    namespace Application.Response.Booking
    {
        public class BookingResponse
        {
            public int Id { get; set; }
            public string BookingCode { get; set; } = string.Empty;

            // User info
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;

            // Tour info
            public int TourId { get; set; }
            public string TourName { get; set; } = string.Empty;
            public string TourLocation { get; set; } = string.Empty;
            public string? TourImageUrl { get; set; }

            // ============ TourDeparture info (NEW) ============
            public int? TourDepartureId { get; set; }
            public DepartureInfoDto? DepartureInfo { get; set; }
            // =================================================

            // Guide info
            public int? GuideId { get; set; }
            public string? GuideName { get; set; }
            public string? GuideAvatar { get; set; }
            public string? GuideEmail { get; set; }
            public string? GuidePhone { get; set; }

            // Booking details
            public DateTime TourDate { get; set; }
            public int NumberOfGuests { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; } = string.Empty;

            // Payment
            public string PaymentStatus { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string? PaymentTransactionId { get; set; }
            public DateTime? PaymentDate { get; set; }

            // Customer
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public string? SpecialRequests { get; set; }

            // Cancellation
            public DateTime? CancelledAt { get; set; }
            public string? CancellationReason { get; set; }
            public decimal? RefundAmount { get; set; }

            // Guests
            public List<BookingGuestResponse> Guests { get; set; } = new();

            // Timestamps
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}
