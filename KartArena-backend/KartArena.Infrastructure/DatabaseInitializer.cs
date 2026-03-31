using KartArena.Domain.Entities.Identity;
using KartArena.Infrastructure.Database;
using KartArena.Infrastructure.Database.Seeders;
using KartArena.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KartArena.Infrastructure;

public static class DatabaseInitializer
{
    /// <summary>
    /// Centralized migration and seeding.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IHostEnvironment env)
    {
        await using var scope = services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserEntity>>();
        if (env.IsTest())
        {
            await ctx.Database.EnsureCreatedAsync();
           
            await DynamicDataSeeder.SeedAsync(ctx, hasher);

            return;
        }

        // SQL Server or similar
        await ctx.Database.MigrateAsync();

        if (env.IsDevelopment())
        {
            
            await DynamicDataSeeder.SeedAsync(ctx, hasher);

        }
    }
}