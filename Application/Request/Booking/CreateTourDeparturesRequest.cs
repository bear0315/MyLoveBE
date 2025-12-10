using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class CreateTourDeparturesRequest
    {
        [Required]
        public int TourId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one departure is required")]
        public List<CreateDepartureDto> Departures { get; set; } = new();
    }
}
