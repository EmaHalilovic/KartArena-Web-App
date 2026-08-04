using KartArena.Application.Abstractions;
using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe;
using KartArena.Infrastructure.Common;
using KartArena.Infrastructure.Database;
using KartArena.Infrastructure.Payments;
using KartArena.Shared.Constants;
using KartArena.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stripe;


namespace KartArena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Typed ConnectionStrings + validation
        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // DbContext: InMemory for test environments; SQL Server otherwise
        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            if (env.IsTest())
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");

                return;
            }

            var cs = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Main;
            options.UseSqlServer(cs);
        });

        //// IAppDbContext mapping
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DatabaseContext>());

        //// Identity hasher
        services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();

        //// stripe
        services.Configure<StripeSettings>(
            configuration.GetSection(StripeSettings.SectionName));

        var stripeSecretKey = configuration["Stripe:SecretKey"];

        if (string.IsNullOrWhiteSpace(stripeSecretKey))
        {
            throw new InvalidOperationException(
                "Stripe secret key is not configured.");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();



        services.AddOptions<JwtOptions>()
        .Bind(configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

        // Token service (reads JwtOptions via IOptions<JwtOptions>)
        services.AddTransient<IJwtTokenService, JwtTokenService>();

        //// HttpContext accessor + current user
        services.AddHttpContextAccessor();
        //services.AddScoped<IAppCurrentUser, AppCurrentUser>();

        //// TimeProvider (if used in handlers/services)
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddHostedService<ExpiredReservationCancellationService>();

        return services;
    }
}