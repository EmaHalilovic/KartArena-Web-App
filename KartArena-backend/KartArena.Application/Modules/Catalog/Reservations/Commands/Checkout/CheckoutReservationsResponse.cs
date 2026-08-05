using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout
{
    public sealed class CheckoutReservationsResponse
    {
        public List<int> ReservationIds { get; set; } = new();

        public int PaymentId { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public string? CheckoutUrl { get; set; }

        public string? StripeCheckoutSessionId { get; set; }
    }
}
