using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Payments
{
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Cancelled = 3,
        Refunded = 4
    }
}
