using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class LoyaltyStatisticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalActiveUsers { get; set; }

        public TierStatistics Bronze { get; set; } = new();
        public TierStatistics Silver { get; set; } = new();
        public TierStatistics Gold { get; set; } = new();

        public int TotalPointsInCirculation { get; set; }
        public string TotalPointsFormatted => $"{TotalPointsInCirculation:N0} điểm";

        public decimal EstimatedPointsValue { get; set; }
        public string EstimatedPointsValueFormatted => $"{EstimatedPointsValue:N0} VND";

        public MonthlyTrend LastMonthTrend { get; set; } = new();
        public MonthlyTrend CurrentMonthTrend { get; set; } = new();

        public List<TopUser> TopUsersByPoints { get; set; } = new();
        public List<TopUser> TopUsersBySpending { get; set; } = new();
    }
}
