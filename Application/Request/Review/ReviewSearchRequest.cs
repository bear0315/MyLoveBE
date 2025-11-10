using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Review
{
    public class ReviewSearchRequest
    {
        public int? TourId { get; set; }
        public int? UserId { get; set; }
        public ReviewStatus? Status { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "created";
        public bool SortDesc { get; set; } = true;
    }
}
