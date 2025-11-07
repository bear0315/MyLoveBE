using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class CreateTourRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DestinationId { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public int MaxGuests { get; set; }
        public TourType Type { get; set; }
        public TourCategory Category { get; set; }
        public TourDifficulty? Difficulty { get; set; }
        public bool IsFeatured { get; set; }
        public string? PhysicalRequirements { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? SpecialRequirements { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        // Related data
        public List<CreateTourImageDto>? Images { get; set; }
        public List<CreateTourItineraryDto>? Itineraries { get; set; }
        public List<string>? Includes { get; set; }
        public List<string>? Excludes { get; set; }
        public List<int>? GuideIds { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
