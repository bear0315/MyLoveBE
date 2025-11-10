using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tag
{
    public class CreateTagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
