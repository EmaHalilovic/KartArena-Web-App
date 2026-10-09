using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe;
using KartArena.Application.Modules.Catalog.Reservations.Services;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;

public sealed class CheckoutReservationsCommandHandler(
    IAppDbContext ctx,
    IStripePaymentService stripePaymentService,
    IReservationRulesService reservationRulesService)
    : IRequestHandler<
        CheckoutReservationsCommand,
        CheckoutReservationsResponse>
{
    private const string StripePaymentCode = "STRIPE";
    private const string DeskCashPaymentCode = "DESK_CASH";
    private const string DefaultCurrency = "bam";

    public async Task<CheckoutReservationsResponse> Handle(
        CheckoutReservationsCommand request,
        CancellationToken ct)
    {
        if (request.Items.Count == 0)
        {
            throw new MarketBusinessRuleException(
                "CART_IS_EMPTY",
                "Cart is empty.");
        }

        if (request.UserId.HasValue)
        {
            var userExists = await ctx.Users.AnyAsync(
                x => x.Id == request.UserId.Value &&
                     !x.IsDeleted,
                ct);

            if (!userExists)
            {
                throw new MarketNotFoundException("User does not exist.");
            }
        }

        if (!request.PaymentTypeId.HasValue)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_REQUIRED",
                "Payment type is required.");
        }

        var paymentType = await ctx.PaymentTypes
            .SingleOrDefaultAsync(
                x => x.Id == request.PaymentTypeId.Value &&
                     !x.IsDeleted,
                ct);

        if (paymentType is null)
        {
            throw new MarketNotFoundException(
                "Selected payment type does not exist.");
        }

        if (!paymentType.isEnabled)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_NOT_ENABLED",
                "Selected payment type is not enabled.");
        }

        if (!paymentType.AllowedOnline)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_NOT_ALLOWED_ONLINE",
                "Selected payment type is not available for online payment.");
        }

        var isDeskCashPayment = string.Equals(
            paymentType.Code,
            DeskCashPaymentCode,
            StringComparison.OrdinalIgnoreCase);

        var ruleRequests = request.Items
        .Select(item => new ReservationRulesRequest(
            item.ReservationDate,
            item.StartTime,
            item.EndTime,
            item.TrackId,
            item.KartId))
        .ToList();

        var ruleResults = await reservationRulesService
            .ValidateBatchAsync(
                ruleRequests,
                ct);


        await using var transaction =
            await ctx.Database.BeginTransactionAsync(ct);
        var transactionCommitted = false;

        try
        {
            var reservationsToCreate =
                new List<ReservationEntity>();

            for (var index = 0;
                 index < request.Items.Count;
                 index++)
            {
                var item = request.Items[index];
                var rulesResult = ruleResults[index];

                var reservation = new ReservationEntity
                {
                    UserId = null,

                    CustomerFirstName =
                        request.CustomerFirstName.Trim(),

                    CustomerLastName =
                        request.CustomerLastName.Trim(),

                    CustomerEmail =
                        request.CustomerEmail.Trim(),

                    CustomerPhone =
                        request.CustomerPhone.Trim(),

                    CustomerNote =
                        string.IsNullOrWhiteSpace(
                            request.CustomerNote)
                            ? null
                            : request.CustomerNote.Trim(),

                    TrackId = item.TrackId,
                    KartId = item.KartId,

                    Date = item.ReservationDate.Date,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,

                    TotalPrice = rulesResult.TotalPrice,

                    Status = isDeskCashPayment
                        ? ReservationStatus.Confirmed
                        : ReservationStatus.Pending,

                    PaymentStatus = PaymentStatus.Pending,

                    IsDeleted = false
                };

                reservationsToCreate.Add(reservation);
            }

            var calculatedTotalAmount =
                reservationsToCreate.Sum(
                    x => x.TotalPrice);

            if (request.Amount.HasValue &&
                request.Amount.Value != calculatedTotalAmount)
            {
                throw new MarketBusinessRuleException(
                    "AMOUNT_MISMATCH",
                    "Submitted amount does not match calculated amount.");
            }

            await ctx.Reservations.AddRangeAsync(
                reservationsToCreate,
                ct);

            var payment = new PaymentEntity
            {
                Amount = calculatedTotalAmount,
                Currency = DefaultCurrency,

                Status = PaymentStatus.Pending,
                PaymentDate = null,

                PaymentTypeId = paymentType.Id,

                Note = string.IsNullOrWhiteSpace(
                    request.PaymentNote)
                    ? null
                    : request.PaymentNote.Trim(),

                IsDeleted = false,
                isEnabled = true
            };

            await ctx.Payments.AddAsync(
                payment,
                ct);

            var paymentReservations =
                reservationsToCreate
                    .Select(reservation =>
                        new PaymentReservationEntity
                        {
                            Payment = payment,
                            Reservation = reservation
                        })
                    .ToList();

            await ctx.PaymentReservations.AddRangeAsync(
                paymentReservations,
                ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            transactionCommitted = true;

            if (string.Equals(
                    paymentType.Code,
                    StripePaymentCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                var stripeResponse =
                    await stripePaymentService
                        .CreateCheckoutSessionAsync(
                            payment.Id,
                            ct);

                return new CheckoutReservationsResponse
                {
                    ReservationIds = reservationsToCreate
                        .Select(x => x.Id)
                        .ToList(),

                    PaymentId = payment.Id,

                    PaymentStatus =
                        payment.Status.ToString(),

                    CheckoutUrl =
                        stripeResponse.CheckoutUrl,

                    StripeCheckoutSessionId =
                        stripeResponse.SessionId
                };
            }

            return new CheckoutReservationsResponse
            {
                ReservationIds = reservationsToCreate
                    .Select(x => x.Id)
                    .ToList(),

                PaymentId = payment.Id,

                PaymentStatus =
                    payment.Status.ToString(),

                CheckoutUrl = null,

                StripeCheckoutSessionId = null
            };
        }
        catch
        {
            if (!transactionCommitted)
            {
                await transaction.RollbackAsync(ct);
            }
            throw;
        }
    }

   
}
