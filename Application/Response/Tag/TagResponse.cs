using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Tag
{
    public class TagResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int TourCount { get; set; }
    }
}
