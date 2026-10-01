using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Application.AccessPoints;
using SmartBuilding.Application.Buildings;
using SmartBuilding.Application.Floors;
using SmartBuilding.Application.Users;
using SmartBuilding.Infrastructure.Data;
using SmartBuilding.Infrastructure.Data.Repositories;
using SmartBuilding.Infrastructure.Data.Seeding;
using SmartBuilding.Infrastructure.Security;

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
        services.AddScoped<IAccessPointRepository, AccessPointRepository>();
        services.AddScoped<IFloorRepository, FloorRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHashService, IdentityPasswordHashService>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}