using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GuideReview : BaseEntity
    {
        public int UserId { get; set; }
        public int GuideId { get; set; }
        public int BookingId { get; set; } // Phải từ booking thật
        public int Rating { get; set; } // 1-5
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        // Các tiêu chí đánh giá guide
        public int? KnowledgeRating { get; set; } // Kiến thức
        public int? CommunicationRating { get; set; } // Giao tiếp
        public int? FriendlinessRating { get; set; } // Thân thiện
        public int? ProfessionalismRating { get; set; } // Chuyên nghiệp

        // Navigation properties
        public User User { get; set; } = null!;
        public Guide Guide { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}
