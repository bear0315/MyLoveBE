using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class TourStatisticsDto
    {
        public int TotalTours { get; set; }
        public int ActiveTours { get; set; }
        public int FeaturedTours { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public decimal AverageRating { get; set; }
    }
}
