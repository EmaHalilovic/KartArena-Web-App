using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Payments.Stripe.Models
{
    public sealed class CreateCheckoutSessionRequest
    {
        public int ReservationId { get; set; }
    }
}
