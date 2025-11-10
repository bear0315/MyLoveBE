using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Destination
{
    public class DestinationListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal StartingPrice { get; set; }
        public int TourCount { get; set; }
        public bool IsFeatured { get; set; }
    }
}
