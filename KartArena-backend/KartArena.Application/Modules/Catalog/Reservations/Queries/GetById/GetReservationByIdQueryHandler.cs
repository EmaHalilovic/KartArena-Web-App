using KartArena.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.GetById
{
    public sealed class GetReservationByIdQueryHandler(IAppDbContext ctx)
        : IRequestHandler<GetReservationByIdQuery, GetReservationByIdQueryDto>
    {
        public async Task<GetReservationByIdQueryDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await ctx.Reservations
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.Id == request.Id)
                .Select(r => new GetReservationByIdQueryDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TrackId = r.TrackId,
                    KartId = r.KartId,
                    Date = r.Date,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,

                    PaymentAmount = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link => (decimal?)link.Payment.Amount)
                    .FirstOrDefault(),

                    PaymentTypeId = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link => link.Payment.PaymentTypeId)
                    .FirstOrDefault(),

                    PaymentTypeName = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link =>
                        link.Payment.PaymentType != null
                            ? link.Payment.PaymentType.Name
                            : null)
                    .FirstOrDefault(),

                    PaymentDate = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link => link.Payment.PaymentDate)
                    .FirstOrDefault(),

                    TransactionReference = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link =>
                        link.Payment.TransactionReference)
                    .FirstOrDefault(),

                    PaymentNote = r.PaymentReservations
                    .Where(link => !link.Payment.IsDeleted)
                    .OrderByDescending(link =>
                        link.Payment.Status == PaymentStatus.Paid)
                    .ThenByDescending(link =>
                        link.Payment.Status == PaymentStatus.Pending)
                    .ThenByDescending(link => link.Payment.Id)
                    .Select(link => link.Payment.Note)
                    .FirstOrDefault(),

                    UserFirstName = r.User != null ? r.User.FirstName : null,
                    UserLastName = r.User != null ? r.User.LastName : null,
                    CustomerFirstName = r.CustomerFirstName,
                    CustomerLastName = r.CustomerLastName,
                    CustomerEmail = r.CustomerEmail,
                    CustomerPhone = r.CustomerPhone,
                    CustomerNote = r.CustomerNote,
                    TrackName = r.Track != null ? r.Track.Name : null,
                    KartName = r.Kart != null ? r.Kart.Name : null,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                throw new Exception("Reservation not found.");

            return dto;
        }
    }
}
