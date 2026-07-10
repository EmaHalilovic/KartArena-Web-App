using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KartArena.Domain.Common;

namespace KartArena.Domain.Entities.Payments
{
    public class PaymentTypeEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool AllowedOnline { get; set; }

        public bool AllowedAtDesk { get; set; }

        public string? Description { get; set; }

        public ICollection<PaymentEntity> Payments { get; set; }
            = new List<PaymentEntity>();
    }
}