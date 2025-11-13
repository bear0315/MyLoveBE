using Application.Response.Tour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideDetailResponse : GuideListResponse
    {
        public int? UserId { get; set; }
        public string? Bio { get; set; }
        public List<GuideTourDto> Tours { get; set; } = new();
    }
}
