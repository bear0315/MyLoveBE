using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        PartiallyPaid = 2,
        Refunded = 3,
        Failed = 4
    }
}
