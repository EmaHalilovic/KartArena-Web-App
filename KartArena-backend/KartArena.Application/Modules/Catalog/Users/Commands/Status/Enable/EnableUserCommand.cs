using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Status.Enable
{
    public sealed class EnableUserCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
