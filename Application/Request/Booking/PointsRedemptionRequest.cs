using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class PointsRedemptionRequest
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BookingAmount { get; set; } // VND

        [Required]
        [Range(0, int.MaxValue)]
        public int PointsToRedeem { get; set; } 
    }
}
