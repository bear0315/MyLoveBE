using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Review
{
    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
        public List<string>? Images { get; set; }
    }
}
