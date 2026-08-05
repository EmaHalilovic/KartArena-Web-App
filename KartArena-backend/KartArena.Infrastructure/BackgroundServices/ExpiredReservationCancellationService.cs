using KartArena.Application.Abstractions;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class ExpiredReservationCancellationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExpiredReservationCancellationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var now = DateTime.UtcNow;

            var expirationThreshold = now.AddMinutes(-10);

            var expiredReservations = await context.Reservations
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == ReservationStatus.Pending &&
                    x.PaymentStatus != PaymentStatus.Paid &&
                    x.CreatedAtUtc <= expirationThreshold)
                .ToListAsync(stoppingToken);

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.PaymentStatus = PaymentStatus.Cancelled;
                reservation.ModifiedAtUtc = now;
            }

            await context.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}