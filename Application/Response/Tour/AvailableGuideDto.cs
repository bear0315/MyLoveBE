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
        public string? Bio { get; set; }
        public string? Languages { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalTours { get; set; }
        public bool IsDefault { get; set; }
        public bool IsAvailable { get; set; }
        public List<string> Specialties { get; set; } = new();
    }
}
