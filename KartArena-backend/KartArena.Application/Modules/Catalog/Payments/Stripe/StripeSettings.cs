using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Payments.Stripe
{
    public sealed class StripeSettings
    {
        public const string SectionName = "Stripe";

        public string SecretKey { get; set; }
            = string.Empty;

        public string WebhookSecret { get; set; }
            = string.Empty;
    }
}
