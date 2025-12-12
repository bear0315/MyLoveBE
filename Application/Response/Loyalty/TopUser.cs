using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class TopUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public MemberTier Tier { get; set; }
        public int Points { get; set; }
        public string PointsFormatted => $"{Points:N0} điểm";
        public decimal? TotalSpent { get; set; }
        public string TotalSpentFormatted => TotalSpent.HasValue ? $"{TotalSpent.Value:N0} VND" : "N/A";
    }
}
