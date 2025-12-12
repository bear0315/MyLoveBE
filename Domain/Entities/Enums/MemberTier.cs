using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum MemberTier
    {
        Bronze = 0,   // Mặc định
        Silver = 1,   // Từ 1000 điểm
        Gold = 2,     // Từ 5000 điểm
        Platinum = 3  // Từ 10000 điểm
    }
}
