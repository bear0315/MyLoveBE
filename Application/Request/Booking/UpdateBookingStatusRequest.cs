using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class UpdateBookingStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, InProgress, Completed, Cancelled
    }
}
