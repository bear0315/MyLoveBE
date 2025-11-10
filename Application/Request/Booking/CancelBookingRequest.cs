using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class CancelBookingRequest
    {
        [Required]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
