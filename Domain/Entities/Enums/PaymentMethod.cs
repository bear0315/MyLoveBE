using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum PaymentMethod
    {
        CreditCard = 0,
        VNPay = 1,
        PayPal = 2,
        BankTransfer = 3,
        Cash = 4
    }
}
