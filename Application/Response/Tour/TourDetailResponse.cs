using Application.Request.Tour;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tour
{
    public class TourDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int DestinationId { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public int MaxGuests { get; set; }
        public TourType Type { get; set; }
        public TourCategory Category { get; set; }
        public TourDifficulty? Difficulty { get; set; }
        public TourStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? PhysicalRequirements { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? SpecialRequirements { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related data
        public List<TourImageDto> Images { get; set; } = new();
        public List<TourItineraryDto> Itineraries { get; set; } = new();
        public List<TourIncludeDto> Includes { get; set; } = new();
        public List<TourExcludeDto> Excludes { get; set; } = new();
        public List<TourGuideDto> Guides { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public List<TourReviewDto> Reviews { get; set; } = new();
    }
}
