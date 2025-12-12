using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class UserLoyaltySummary
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int LoyaltyPoints { get; set; }
        public string LoyaltyPointsFormatted => $"{LoyaltyPoints:N0} điểm";
        public MemberTier MemberTier { get; set; }
        public string MemberTierName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime MemberSince { get; set; }
        public DateTime? LastTierUpdateAt { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
