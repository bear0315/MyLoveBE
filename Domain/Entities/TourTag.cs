using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TourTag : BaseEntity
    {
        public int TourId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public Tour Tour { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
