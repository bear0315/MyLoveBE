using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DailyStatistic : BaseEntity
    {
        public DateTime Date { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewUsers { get; set; }
        public int TotalGuests { get; set; }
        public int ActiveTours { get; set; }
    }
}
