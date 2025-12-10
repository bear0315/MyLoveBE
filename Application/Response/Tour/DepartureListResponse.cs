using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tour
{
    public class DepartureListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TourDepartureResponse> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
