using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Review
{
    public class ReviewSummaryResponse
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int RecommendationPercentage { get; set; } 
        public List<ReviewResponse> RecentReviews { get; set; } = new();
        public ReviewStatisticsResponse Statistics { get; set; } = new();
    }
}
