using KartArena.Application.Abstractions;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KartArena.Infrastructure.BackgroundServices;

public sealed class ExpiredReservationCancellationService
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromMinutes(1);

    private static readonly TimeSpan PaymentExpirationTime =
        TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredReservationCancellationService> _logger;

    public ExpiredReservationCancellationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredReservationCancellationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelExpiredPaymentsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An error occurred while cancelling expired reservations.");
            }

            await Task.Delay(
                CheckInterval,
                stoppingToken);
        }
    }

    private async Task CancelExpiredPaymentsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<IAppDbContext>();

        var now = DateTime.UtcNow;
        var expirationThreshold =
            now.Subtract(PaymentExpirationTime);

        var expiredPayments = await context.Payments
            .Include(payment => payment.PaymentReservations)
                .ThenInclude(link => link.Reservation)
            .Where(payment =>
                !payment.IsDeleted &&
                payment.Status == PaymentStatus.Pending &&
                payment.CreatedAtUtc <= expirationThreshold)
            .ToListAsync(cancellationToken);

        if (expiredPayments.Count == 0)
        {
            return;
        }

        foreach (var payment in expiredPayments)
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.PaymentDate = null;
            payment.ModifiedAtUtc = now;
            payment.Note = string.IsNullOrWhiteSpace(payment.Note)
                ? "Payment expired before completion."
                : $"{payment.Note} Payment expired before completion.";

            foreach (var link in payment.PaymentReservations)
            {
                var reservation = link.Reservation;

                if (reservation.IsDeleted)
                {
                    continue;
                }

                if (reservation.PaymentStatus == PaymentStatus.Paid)
                {
                    continue;
                }

                if (reservation.Status is
                    ReservationStatus.Completed or
                    ReservationStatus.Cancelled)
                {
                    continue;
                }

                reservation.Status =
                    ReservationStatus.Cancelled;

                reservation.PaymentStatus =
                    PaymentStatus.Cancelled;

                reservation.ModifiedAtUtc = now;
            }
        }

        await context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Cancelled {PaymentCount} expired payments.",
            expiredPayments.Count);
    }
}