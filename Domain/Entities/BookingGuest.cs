    using Domain.Entities.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Domain.Entities
    {
        public class BookingGuest : BaseEntity
        {
            public int BookingId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public DateTime DateOfBirth { get; set; }
            public string Gender { get; set; } = string.Empty;
            public string? PassportNumber { get; set; }
            public string? Nationality { get; set; }
            public string? SpecialRequirements { get; set; }

            // noi bang 
            public Booking Booking { get; set; } = null!;
        }
    }
