using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response.Loyalty
{
    public class AdjustPointsResult
    {
        public int NewTotalPoints { get; set; }
        public MemberTier PreviousTier { get; set; }
        public MemberTier NewTier { get; set; }
        public bool TierChanged { get; set; }
    }
}
