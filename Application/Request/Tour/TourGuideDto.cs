using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class TourGuideDto
    {
        public int GuideId { get; set; }
        public string GuideName { get; set; } = string.Empty;
        public string? GuideAvatar { get; set; }
        public decimal GuideRating { get; set; }
        public bool IsDefault { get; set; }
    }
}
