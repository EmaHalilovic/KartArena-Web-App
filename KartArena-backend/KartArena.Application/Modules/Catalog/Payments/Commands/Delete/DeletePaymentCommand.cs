using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Delete
{
    public sealed class DeletePaymentCommand:IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
