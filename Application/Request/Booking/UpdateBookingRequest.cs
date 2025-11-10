using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class UpdateBookingRequest
    {
        [MaxLength(255)]
        public string? CustomerName { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? CustomerEmail { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }

        public List<BookingGuestRequest>? Guests { get; set; }
    }
}
