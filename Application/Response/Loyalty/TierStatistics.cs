using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class TierStatistics
    {
        public int UserCount { get; set; }
        public decimal Percentage { get; set; }
        public string PercentageFormatted => $"{Percentage:F1}%";
        public int TotalPoints { get; set; }
        public decimal AveragePoints { get; set; }
    }
}
