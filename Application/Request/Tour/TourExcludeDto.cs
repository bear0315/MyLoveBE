using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class TourExcludeDto
    {
        public int Id { get; set; }
        public string Item { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
