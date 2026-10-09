using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;
using KartArena.Application.Modules.Catalog.Payments.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        
    }
}
