using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tour
{
    public class TourListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public TourType Type { get; set; }
        public TourCategory Category { get; set; }
        public TourStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookings { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<TourDepartureDto> Departures { get; set; } = new();

    }
}
