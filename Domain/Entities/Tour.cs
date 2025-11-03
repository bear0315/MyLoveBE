using Domain.Entities.Common;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Tour : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DestinationId { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty; // "5 ngày 4 đêm"
        public int DurationDays { get; set; } // Số ngày để sort/filter
        public int MaxGuests { get; set; }

        // Tour Type & Category
        public TourType Type { get; set; } = TourType.Standard;
        public TourCategory Category { get; set; }

        public TourDifficulty? Difficulty { get; set; }

        public TourStatus Status { get; set; } = TourStatus.Active;
        public bool IsFeatured { get; set; } = false;
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int TotalBookings { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        // Additional attributes
        public string? PhysicalRequirements { get; set; } // Yêu cầu thể chất (nếu có)
        public int? MinAge { get; set; } // Độ tuổi tối thiểu
        public int? MaxAge { get; set; } // Độ tuổi tối đa (nếu có)
        public string? SpecialRequirements { get; set; } // Yêu cầu đặc biệt

        // SEO
        public string Slug { get; set; } = string.Empty;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        // Navigation properties
        public Destination Destination { get; set; } = null!;
        public ICollection<TourImage> Images { get; set; } = new List<TourImage>();
        public ICollection<TourItinerary> Itineraries { get; set; } = new List<TourItinerary>();
        public ICollection<TourInclude> Includes { get; set; } = new List<TourInclude>();
        public ICollection<TourExclude> Excludes { get; set; } = new List<TourExclude>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<TourGuide> TourGuides { get; set; } = new List<TourGuide>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<TourTag> TourTags { get; set; } = new List<TourTag>();
    }

}
