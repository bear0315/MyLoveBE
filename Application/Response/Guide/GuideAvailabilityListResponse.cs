using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Guide
{
    public class GuideAvailabilityListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GuideAvailabilityResponse> Data { get; set; } = new();
    }
}
