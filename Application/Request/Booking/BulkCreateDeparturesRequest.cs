using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class BulkCreateDeparturesRequest
    {
        [Required]
        public int TourId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Pattern: Daily, Weekly, BiWeekly, Monthly
        /// </summary>
        [Required]
        public string Pattern { get; set; } = "Weekly";

        /// <summary>
        /// Cho pattern Weekly: Monday, Tuesday, etc.
        /// </summary>
        public List<string>? DaysOfWeek { get; set; }

        /// <summary>
        /// Max guests override (null = dùng tour.MaxGuests)
        /// </summary>
        public int? MaxGuestsOverride { get; set; }
    }
}
