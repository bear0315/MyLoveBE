using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminUserLoyaltyDetailResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int CurrentPoints { get; set; }
        public string CurrentPointsFormatted => $"{CurrentPoints:N0} điểm";
        public MemberTier CurrentTier { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string DiscountPercentageFormatted => $"{DiscountPercentage * 100}%";

        // Tier progression
        public MemberTier? NextTier { get; set; }
        public string? NextTierName { get; set; }
        public int? PointsToNextTier { get; set; }
        public string? PointsToNextTierFormatted => PointsToNextTier.HasValue
            ? $"{PointsToNextTier.Value:N0} điểm"
            : "Đã đạt hạng cao nhất";

        // Statistics
        public int TotalPointsEarned { get; set; }
        public string TotalPointsEarnedFormatted => $"{TotalPointsEarned:N0} điểm";
        public int TotalPointsRedeemed { get; set; }
        public string TotalPointsRedeemedFormatted => $"{TotalPointsRedeemed:N0} điểm";
        public int TotalTransactions { get; set; }

        // Dates
        public DateTime MemberSince { get; set; }
        public string MemberSinceFormatted => MemberSince.ToString("dd/MM/yyyy");
        public DateTime? LastTierUpdateAt { get; set; }
        public string? LastTierUpdateAtFormatted => LastTierUpdateAt?.ToString("dd/MM/yyyy HH:mm");
        public DateTime? LastTransactionAt { get; set; }
        public string? LastTransactionAtFormatted => LastTransactionAt?.ToString("dd/MM/yyyy HH:mm");
    }
}
