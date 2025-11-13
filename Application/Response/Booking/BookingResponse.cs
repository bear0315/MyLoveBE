using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Booking
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string TourLocation { get; set; } = string.Empty;
        public int? GuideId { get; set; }
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
        public string? GuideEmail { get; set; }

        // Booking Details
        public DateTime TourDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        // Payment Details
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Customer Information
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
