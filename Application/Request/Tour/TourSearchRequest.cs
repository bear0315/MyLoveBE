using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class TourSearchRequest
    {
        public string? Keyword { get; set; }
        public int? DestinationId { get; set; }
        public TourCategory? Category { get; set; }
        public TourType? Type { get; set; }
        public TourDifficulty? Difficulty { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinDays { get; set; }
        public int? MaxDays { get; set; }
        public int? MinRating { get; set; }
        public List<int>? TagIds { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "created";
        public bool SortDesc { get; set; } = true;
    }
}
