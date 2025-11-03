using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TourItinerary : BaseEntity
    {
        public int TourId { get; set; }
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Activities { get; set; }
        public string? Meals { get; set; } // "Breakfast, Lunch"
        public string? Accommodation { get; set; }

        // Navigation property
        public Tour Tour { get; set; } = null!;
    }
}
