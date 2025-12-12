using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdminPointsHistoryResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? BookingCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
