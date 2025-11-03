using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int TourId { get; set; }
        public int? BookingId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public int HelpfulCount { get; set; } = 0;

        // Navigation properties
        public User User { get; set; } = null!;
        public Tour Tour { get; set; } = null!;
        public Booking? Booking { get; set; }
        public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
    }
}
