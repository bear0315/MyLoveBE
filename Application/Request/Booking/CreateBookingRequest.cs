using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class CreateBookingRequest
    {
        [Required]
        public int TourId { get; set; }

        public int? TourDepartureId { get; set; }

        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Number of guests must be between 1 and 100")]
        public int NumberOfGuests { get; set; }
       
        [Range(0, int.MaxValue, ErrorMessage = "Points to redeem must be non-negative")]
        public int? PointsToRedeem { get; set; }
        public int? GuideId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public List<BookingGuestRequest> Guests { get; set; } = new();
    }
}
