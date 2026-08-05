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

                    PaymentAmount = r.Payment != null ? r.Payment.Amount : null,
                    PaymentTypeId = r.Payment != null ? r.Payment.PaymentTypeId : null,
                    PaymentTypeName = r.Payment != null && r.Payment.PaymentType != null ? r.Payment.PaymentType.Name : null,
                    PaymentDate = r.Payment != null ? r.Payment.PaymentDate : null,
                    TransactionReference = r.Payment != null ? r.Payment.TransactionReference : null,
                    PaymentNote = r.Payment != null ? r.Payment.Note : null,

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
