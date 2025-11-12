using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class AssignGuideRequest
    {
        [Required]
        public int GuideId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
