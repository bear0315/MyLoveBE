using Application.Request.Review;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Review
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public List<ReviewImageDto>? Images { get; set; }
    }
}
