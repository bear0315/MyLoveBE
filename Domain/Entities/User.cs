using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime? LastLoginAt { get; set; }
        public DateTime MemberSince { get; set; } = DateTime.UtcNow;
        public int LoyaltyPoints { get; set; } = 0;
        public MemberTier MemberTier { get; set; } = MemberTier.Bronze;
        public DateTime? LastTierUpdateAt { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<GuideReview> GuideReviews { get; set; } = new List<GuideReview>(); 
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>(); 
        public Guide? GuideProfile { get; set; }
        public ICollection<PointsHistory> PointsHistories { get; set; } = new List<PointsHistory>();

    }

}
