using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Booking : BaseEntity
    {
        public string BookingCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int TourId { get; set; }
        public int? GuideId { get; set; }

        // Booking Details
        public DateTime TourDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Payment Details
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
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
        
        // noi bang 
        public User User { get; set; } = null!;
        public Tour Tour { get; set; } = null!;
        public Guide? Guide { get; set; }
        public ICollection<BookingGuest> Guests { get; set; } = new List<BookingGuest>();
        public Review? TourReview { get; set; } 
        public GuideReview? GuideReview { get; set; } 
    }

}
