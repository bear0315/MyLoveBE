using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class MemberTierInfo
    {
        public string TierName { get; set; } = string.Empty;
        public int TierLevel { get; set; }
        public int MinPoints { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string DiscountPercentageFormatted => $"{DiscountPercentage * 100}%";
        public List<string> Benefits { get; set; } = new();
    }
}
