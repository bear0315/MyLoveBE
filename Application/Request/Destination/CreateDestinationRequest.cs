using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Destination
{
    public class CreateDestinationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }
}
