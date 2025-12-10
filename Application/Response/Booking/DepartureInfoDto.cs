using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Booking
{
    public class DepartureInfoDto
    {
        public int Id { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxGuests { get; set; }
        public int BookedGuests { get; set; }
        public int AvailableSlots { get; set; }
        public decimal? SpecialPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
