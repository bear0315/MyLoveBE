using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class BulkUpdateStatusRequest
    {
        public List<int> TourIds { get; set; } = new();
        public TourStatus Status { get; set; }
    }
}
