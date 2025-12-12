using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminAllPointsHistoryResponse
    {
        public List<AdminPointsHistoryItem> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    public class AdminPointsHistoryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public string PointsFormatted => $"{(Points >= 0 ? "+" : "")}{Points:N0} điểm";
        public string Description { get; set; } = string.Empty;
        public string? BookingCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }
   

    public class MemberTierDistribution
    {
        public int TotalMembers { get; set; }
        public int BronzeCount { get; set; }
        public int SilverCount { get; set; }
        public int GoldCount { get; set; }
        public decimal BronzePercentage => TotalMembers > 0 ? (decimal)BronzeCount / TotalMembers * 100 : 0;
        public decimal SilverPercentage => TotalMembers > 0 ? (decimal)SilverCount / TotalMembers * 100 : 0;
        public decimal GoldPercentage => TotalMembers > 0 ? (decimal)GoldCount / TotalMembers * 100 : 0;
        public string BronzePercentageFormatted => $"{BronzePercentage:F1}%";
        public string SilverPercentageFormatted => $"{SilverPercentage:F1}%";
        public string GoldPercentageFormatted => $"{GoldPercentage:F1}%";
    }

    public class PointsStatistics
    {
        public long TotalPointsInSystem { get; set; }
        public string TotalPointsInSystemFormatted => $"{TotalPointsInSystem:N0} điểm";
        public long TotalPointsEarned { get; set; }
        public string TotalPointsEarnedFormatted => $"{TotalPointsEarned:N0} điểm";
        public long TotalPointsRedeemed { get; set; }
        public string TotalPointsRedeemedFormatted => $"{TotalPointsRedeemed:N0} điểm";
        public decimal AveragePointsPerUser { get; set; }
        public string AveragePointsPerUserFormatted => $"{AveragePointsPerUser:N0} điểm";
    }

    public class ActivityStatistics
    {
        public int TotalTransactions { get; set; }
        public int TransactionsLast30Days { get; set; }
        public int EarnedTransactionsLast30Days { get; set; }
        public int RedeemedTransactionsLast30Days { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public string? LastTransactionDateFormatted => LastTransactionDate?.ToString("dd/MM/yyyy HH:mm");
    }

    public class TopUserByPoints
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public string CurrentPointsFormatted => $"{CurrentPoints:N0} điểm";
        public MemberTier CurrentTier { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
    }
}
