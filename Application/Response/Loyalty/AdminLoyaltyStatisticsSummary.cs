using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminLoyaltyStatisticsSummary
    {
        public int TotalBronzeMembers { get; set; }
        public int TotalSilverMembers { get; set; }
        public int TotalGoldMembers { get; set; }
        public int TotalActiveMembers { get; set; }
        public long TotalPointsInSystem { get; set; }
        public string TotalPointsInSystemFormatted => $"{TotalPointsInSystem:N0} điểm";
    }
}
