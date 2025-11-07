using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class CreateTourItineraryDto
    {
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Activities { get; set; }
        public string? Meals { get; set; }
        public string? Accommodation { get; set; }
    }
}
