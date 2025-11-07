using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class BulkUpdateFeaturedRequest
    {
        public List<int> TourIds { get; set; } = new();
        public bool IsFeatured { get; set; }
    }
}
