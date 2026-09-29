using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Infrastructure.Data.Seeding;

namespace SmartBuilding.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDevelopmentDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        SmartBuildingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<SmartBuildingDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        DevelopmentDataSeeder seeder =
            scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}