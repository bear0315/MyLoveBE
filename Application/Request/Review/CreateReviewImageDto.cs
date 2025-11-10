using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Review
{
    public class CreateReviewImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
