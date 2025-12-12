using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class LoyaltyInfoResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public string CurrentPointsFormatted => $"{CurrentPoints:N0} điểm";
        public MemberTier CurrentTier { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string DiscountPercentageFormatted => $"{DiscountPercentage * 100}%";

        // Thông tin tier tiếp theo
        public MemberTier? NextTier { get; set; }
        public string? NextTierName { get; set; }
        public int? PointsToNextTier { get; set; }
        public string? PointsToNextTierFormatted => PointsToNextTier.HasValue
            ? $"{PointsToNextTier.Value:N0} điểm"
            : null;

        public DateTime MemberSince { get; set; }
        public DateTime? LastTierUpdateAt { get; set; }
    }
}
