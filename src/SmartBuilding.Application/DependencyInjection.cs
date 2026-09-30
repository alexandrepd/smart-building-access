using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Application.Buildings;

namespace SmartBuilding.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBuildingService, BuildingService>();
        return services;
    }
}