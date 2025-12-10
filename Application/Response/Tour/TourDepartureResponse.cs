using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tour
{
    public class TourDepartureResponse
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxGuests { get; set; }
        public int BookedGuests { get; set; }
        public int AvailableSlots { get; set; }
        public decimal Price { get; set; }  // Effective price (special or default)
        public bool HasSpecialPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? DefaultGuideId { get; set; }
        public string? DefaultGuideName { get; set; }
    }
}
