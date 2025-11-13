using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideListResponse
    {
        public int GuideId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? Languages { get; set; }
        public int ExperienceYears { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
