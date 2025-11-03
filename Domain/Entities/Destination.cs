using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Destination : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public decimal StartingPrice { get; set; }
        public int TourCount { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // SEO
        public string Slug { get; set; } = string.Empty;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        // Navigation properties
        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
