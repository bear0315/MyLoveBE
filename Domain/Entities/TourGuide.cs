using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TourGuide : BaseEntity
    {
        public int TourId { get; set; }
        public int GuideId { get; set; }
        public bool IsDefault { get; set; } = false;

        // Navigation properties
        public Tour Tour { get; set; } = null!;
        public Guide Guide { get; set; } = null!;
    }
}
