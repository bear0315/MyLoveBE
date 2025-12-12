using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PointsHistory : BaseEntity
    {
        public int UserId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Earned", "Redeemed", "Expired"
        public int Points { get; set; } // Có thể âm nếu là Redeemed
        public string Description { get; set; } = string.Empty;
        public string? BookingCode { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
