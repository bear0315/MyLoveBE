using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class MonthlyTrend
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public int NewMembers { get; set; }
        public int TierUpgrades { get; set; }
    }
}
