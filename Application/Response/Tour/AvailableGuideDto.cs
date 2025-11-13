using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tour
{
    public class AvailableGuideDto
    {

        public int GuideId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDefaultGuide { get; set; }

        public string? UnavailabilityReason { get; set; }
        public string? ConflictingBookingCode { get; set; }
        public int? ConflictingBookingId { get; set; }
        public string Bio { get; set; } = string.Empty; 

        public decimal? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public string? Languages { get; set; }
    }
}
