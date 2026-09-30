using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartBuilding.Api.Contracts.Buildings;
using SmartBuilding.Application.Buildings;

namespace SmartBuilding.Api.Endpoints;

internal static class BuildingEndpoints
{
    internal static IEndpointRouteBuilder MapBuildingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/buildings")
            .WithTags("Buildings");

        group.MapGet("/", GetAllAsync)
            .WithName("GetBuildings")
            .WithSummary("List buildings")
            .Produces<IReadOnlyList<BuildingResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetBuildingById")
            .WithSummary("Get a building by ID")
            .Produces<BuildingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateBuilding")
            .WithSummary("Create a building")
            .Produces<BuildingResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateBuilding")
            .WithSummary("Replace a building's editable state")
            .Produces<BuildingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteBuilding")
            .WithSummary("Delete a building without floors")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<BuildingResponse>>> GetAllAsync(
        IBuildingService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BuildingDto> buildings = await service.GetAllAsync(cancellationToken);
        IReadOnlyList<BuildingResponse> response = buildings
            .Select(BuildingResponse.FromDto)
            .ToArray();

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<BuildingResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IBuildingService service,
        CancellationToken cancellationToken)
    {
        BuildingDto? building = await service.GetByIdAsync(id, cancellationToken);
        return building is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(BuildingResponse.FromDto(building));
    }

    private static async Task<Created<BuildingResponse>> CreateAsync(
        CreateBuildingRequest request,
        IBuildingService service,
        CancellationToken cancellationToken)
    {
        BuildingDto building = await service.CreateAsync(
            new CreateBuildingCommand(request.Name, request.Address),
            cancellationToken);
        BuildingResponse response = BuildingResponse.FromDto(building);

        return TypedResults.Created($"/api/buildings/{response.Id}", response);
    }

    private static async Task<Results<Ok<BuildingResponse>, NotFound>> UpdateAsync(
        Guid id,
        UpdateBuildingRequest request,
        IBuildingService service,
        CancellationToken cancellationToken)
    {
        BuildingDto? building = await service.UpdateAsync(
            id,
            new UpdateBuildingCommand(request.Name, request.Address, request.IsActive),
            cancellationToken);

        return building is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(BuildingResponse.FromDto(building));
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IBuildingService service,
        CancellationToken cancellationToken)
    {
        DeleteBuildingResult result = await service.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteBuildingResult.Deleted => TypedResults.NoContent(),
            DeleteBuildingResult.NotFound => TypedResults.NotFound(),
            DeleteBuildingResult.HasFloors => TypedResults.Problem(
                title: "Building cannot be deleted.",
                detail: "Remove or reassign its floors before deleting the building.",
                statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException($"Unsupported delete result: {result}.")
        };
    }
}