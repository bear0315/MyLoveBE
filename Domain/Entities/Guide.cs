using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Guide : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? Languages { get; set; }
        public int ExperienceYears { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public GuideStatus Status { get; set; } = GuideStatus.Active;

        // Link to User Account
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Navigation properties
        public ICollection<TourGuide> TourGuides { get; set; } = new List<TourGuide>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<GuideReview> GuideReviews { get; set; } = new List<GuideReview>(); 
    }
}
