using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideTourDto
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string? TourImage { get; set; }
        public decimal TourRating { get; set; }
        public bool IsDefault { get; set; }
    }
}
