using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Request.User
{
    public class LoyaltyInfoDto
    {
        public int CurrentPoints { get; set; }
        public MemberTier CurrentTier { get; set; }
        public decimal DiscountPercentage { get; set; }
        public MemberTier? NextTier { get; set; }
        public int PointsToNextTier { get; set; }
    }
}
