using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class BookingGuestRequest
    {
        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty; 

        [MaxLength(50)]
        public string? PassportNumber { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        [MaxLength(500)]
        public string? SpecialRequirements { get; set; }
    }
}
