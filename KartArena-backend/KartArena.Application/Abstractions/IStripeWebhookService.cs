using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Abstractions
{
    public interface IStripeWebhookService
    {
        Task ProcessWebhookAsync(
            string json,
            string stripeSignature,
            CancellationToken cancellationToken);
    }
}
