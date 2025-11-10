using Application.Response.Tour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Destination
{
    public class DestinationDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal StartingPrice { get; set; }
        public int TourCount { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public List<TourListResponse>? Tours { get; set; }
    }
}
