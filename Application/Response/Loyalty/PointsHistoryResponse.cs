using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class PointsHistoryResponse
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Earned", "Redeemed", "Expired"
        public int Points { get; set; }
        public string PointsFormatted => $"{(Points >= 0 ? "+" : "")}{Points:N0} điểm";
        public string Description { get; set; } = string.Empty;
        public string? BookingCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }
}
