using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideAvailabilityResponse
    {
        public int GuideId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public bool IsDefaultGuide { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
        public int TotalBookingsOnDate { get; set; }
    }

}
