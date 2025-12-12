using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Loyalty
{
    public class AdminAdjustPointsRequest
    {
        public int UserId { get; set; }
        public int Points { get; set; } // Có thể âm để trừ điểm
        public string Reason { get; set; } = string.Empty;
    }
}
