using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Application.Buildings;
using SmartBuilding.Application.Floors;

namespace SmartBuilding.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBuildingService, BuildingService>();
        services.AddScoped<IFloorService, FloorService>();
        return services;
    }
}