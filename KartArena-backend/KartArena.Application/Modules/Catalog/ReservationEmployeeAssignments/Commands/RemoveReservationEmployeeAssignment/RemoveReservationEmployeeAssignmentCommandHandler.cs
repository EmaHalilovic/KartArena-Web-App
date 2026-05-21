using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Commands.RemoveReservationEmployeeAssignment;

public class RemoveReservationEmployeeAssignmentCommandHandler : IRequestHandler<RemoveReservationEmployeeAssignmentCommand>
{
    private readonly IAppDbContext _context;

    public RemoveReservationEmployeeAssignmentCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveReservationEmployeeAssignmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ReservationEmployees
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException("Assignment not found.");

        entity.IsDeleted = true;
        entity.ModifiedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
