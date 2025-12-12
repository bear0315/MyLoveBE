using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminLoyaltyOverviewResponse
    {
        public List<AdminUserLoyaltySummary> Users { get; set; } = new();
        public AdminLoyaltyStatisticsSummary Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
