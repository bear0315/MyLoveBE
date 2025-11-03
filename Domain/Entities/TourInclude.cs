using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TourInclude : BaseEntity
    {
        public int TourId { get; set; }
        public string Item { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;

        // Navigation property
        public Tour Tour { get; set; } = null!;
    }
}
