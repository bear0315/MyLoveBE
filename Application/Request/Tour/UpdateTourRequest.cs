using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.Tour
{
    public class UpdateTourRequest : CreateTourRequest
    {
        public int Id { get; set; }
    }
}
