using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Favorite
{
    public class FavoriteResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string TourSlug { get; set; } = string.Empty;
        public string? TourImageUrl { get; set; }
        public decimal TourPrice { get; set; }
        public string TourDuration { get; set; } = string.Empty;
        public decimal TourRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
