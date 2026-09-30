using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Application.Buildings;
using SmartBuilding.Infrastructure.Data;
using SmartBuilding.Infrastructure.Data.Repositories;
using SmartBuilding.Infrastructure.Data.Seeding;

namespace SmartBuilding.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<SmartBuildingDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());
            services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}