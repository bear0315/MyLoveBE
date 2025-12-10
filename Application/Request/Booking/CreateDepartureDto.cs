using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class CreateDepartureDto
    {
        [Required]
        public DateTime DepartureDate { get; set; }

        /// <summary>
        /// Số khách tối đa cho ngày này. 
        /// Null = dùng Tour.MaxGuests
        /// </summary>
        public int? MaxGuests { get; set; }

        /// <summary>
        /// Giá đặc biệt cho ngày này.
        /// Null = dùng Tour.Price
        /// </summary>
        public decimal? SpecialPrice { get; set; }

        /// <summary>
        /// Guide mặc định cho ngày này
        /// </summary>
        public int? DefaultGuideId { get; set; }

        public string? Notes { get; set; }
    }
}
