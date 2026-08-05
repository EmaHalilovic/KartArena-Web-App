using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.GetById;

public sealed class GetPaymentByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentByIdQuery, GetPaymentByIdQueryDto>
{
    public async Task<GetPaymentByIdQueryDto> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Id == request.Id &&
                !x.IsDeleted)
            .Select(x => new GetPaymentByIdQueryDto
            {
                Id = x.Id,

                Reservations = x.PaymentReservations
                    .OrderBy(link => link.Reservation.Date)
                    .ThenBy(link => link.Reservation.StartTime)
                    .Select(link => new GetPaymentReservationDto
                    {
                        Id = link.Reservation.Id,

                        CustomerName =
                            (
                                link.Reservation.CustomerFirstName +
                                " " +
                                link.Reservation.CustomerLastName
                            ).Trim(),

                        CustomerEmail =
                            link.Reservation.CustomerEmail,

                        Date =
                            link.Reservation.Date,

                        StartTime =
                            link.Reservation.StartTime,

                        EndTime =
                            link.Reservation.EndTime,

                        TrackName =
                            link.Reservation.Track != null
                                ? link.Reservation.Track.Name
                                : string.Empty,

                        KartName =
                            link.Reservation.Kart != null
                                ? link.Reservation.Kart.Name
                                : string.Empty,

                        TotalPrice =
                            link.Reservation.TotalPrice
                    })
                    .ToList(),

                Amount = x.Amount,

                Currency = x.Currency,

                PaymentDate = x.PaymentDate,

                PaymentTypeId = x.PaymentTypeId,

                PaymentTypeName =
                    x.PaymentType != null
                        ? x.PaymentType.Name
                        : null,

                Status = x.Status,

                TransactionReference =
                    x.TransactionReference,

                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            throw new MarketNotFoundException(
                $"Payment with id {request.Id} was not found.");
        }

        return dto;
    }
}