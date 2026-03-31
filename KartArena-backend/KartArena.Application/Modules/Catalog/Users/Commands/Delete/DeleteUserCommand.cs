using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Delete
{
    public sealed class DeleteUserCommand:IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
