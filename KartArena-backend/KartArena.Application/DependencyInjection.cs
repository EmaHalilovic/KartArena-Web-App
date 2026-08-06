using KartArena.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using KartArena.Application.Modules.Reports;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace KartArena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR only from the Application layer
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation from the Application layer
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors (npr. ValidationBehavior)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // TimeProvider — if used by handlers
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<PdfReportService>();
        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}
