using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Booking
{
    public class UpdatePaymentRequest
    {
        [Required]
        public string PaymentStatus { get; set; } = string.Empty; // Pending, Paid, Failed, Refunded

        [MaxLength(255)]
        public string? PaymentTransactionId { get; set; }

        public DateTime? PaymentDate { get; set; }

        public decimal? RefundAmount { get; set; }
    }
}
