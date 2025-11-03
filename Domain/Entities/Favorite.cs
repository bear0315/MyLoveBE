using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Favorite : BaseEntity
    {
        public int UserId { get; set; }
        public int TourId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Tour Tour { get; set; } = null!;
    }
}
