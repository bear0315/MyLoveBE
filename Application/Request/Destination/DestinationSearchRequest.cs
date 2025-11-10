using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Destination
{
    public class DestinationSearchRequest
    {
        public string? Keyword { get; set; }
        public string? Country { get; set; }
        public bool? IsFeatured { get; set; }
        public decimal? MinRating { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "name";
        public bool SortDesc { get; set; } = false;
    }
}
