using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.Occupancy;
using SmartBuilding.Api.Security;
using SmartBuilding.Application.Occupancy;

namespace SmartBuilding.Api.Endpoints;

internal static class OccupancyEndpoints
{
    internal static IEndpointRouteBuilder MapOccupancyEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/occupancy/current", GetCurrentOccupancyAsync)
            .WithTags("Occupancy")
            .WithName("GetCurrentOccupancy")
            .WithSummary("Get current occupancy by floor")
            .WithDescription(
                "Returns every floor, including floors with no occupants, and the active sessions with their elapsed durations.")
            .Produces<IReadOnlyList<CurrentOccupancyResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization(AuthorizationPolicies.AccessOperations);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<CurrentOccupancyResponse>>> GetCurrentOccupancyAsync(
        IOccupancyService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CurrentOccupancyFloorDto> occupancy =
            await service.GetCurrentOccupancyAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<CurrentOccupancyResponse>>(
            occupancy.Select(CurrentOccupancyResponse.FromDto).ToArray());
    }
}
