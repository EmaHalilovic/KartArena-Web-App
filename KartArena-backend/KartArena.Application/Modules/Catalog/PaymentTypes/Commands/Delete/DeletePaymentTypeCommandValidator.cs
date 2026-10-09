using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class DeletePaymentTypeCommandValidator : AbstractValidator<DeletePaymentTypeCommand>
{
    public DeletePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        
    }
}
