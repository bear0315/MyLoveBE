using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideProfileResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? Languages { get; set; }
        public string? Experience { get; set; }
        public int ExperienceYears { get; set; }
        public string? Specialties { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
