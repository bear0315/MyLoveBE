using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminUserLoyaltySummary
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public string CurrentPointsFormatted => $"{CurrentPoints:N0} điểm";
        public MemberTier CurrentTier { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string DiscountPercentageFormatted => $"{DiscountPercentage * 100}%";
        public int TotalPointsEarned { get; set; }
        public string TotalPointsEarnedFormatted => $"{TotalPointsEarned:N0} điểm";
        public int TotalPointsRedeemed { get; set; }
        public string TotalPointsRedeemedFormatted => $"{TotalPointsRedeemed:N0} điểm";
        public DateTime MemberSince { get; set; }
        public string MemberSinceFormatted => MemberSince.ToString("dd/MM/yyyy");
        public DateTime? LastTierUpdateAt { get; set; }
        public string? LastTierUpdateAtFormatted => LastTierUpdateAt?.ToString("dd/MM/yyyy HH:mm");
    }
}
