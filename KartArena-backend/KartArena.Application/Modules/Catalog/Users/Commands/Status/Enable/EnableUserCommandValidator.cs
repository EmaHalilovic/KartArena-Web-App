using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Status.Enable
{
    public sealed class EnableUserCommandValidator : AbstractValidator<EnableUserCommand>
    {
        public EnableUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("User Id must be greater than 0.");
        }
    }
}
