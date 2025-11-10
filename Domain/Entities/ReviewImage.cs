using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ReviewImage : BaseEntity
    {
        public int ReviewId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public Review Review { get; set; } = null!;
    }
}
