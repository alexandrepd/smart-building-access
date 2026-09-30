using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.Floors;
using SmartBuilding.Application.Floors;

namespace SmartBuilding.Api.Endpoints;

internal static class FloorEndpoints
{
    internal static IEndpointRouteBuilder MapFloorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/floors")
            .WithTags("Floors");

        group.MapGet("/", GetAllAsync)
            .WithName("GetFloors")
            .WithSummary("List floors")
            .Produces<IReadOnlyList<FloorResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetFloorById")
            .WithSummary("Get a floor by ID")
            .Produces<FloorResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateFloor")
            .WithSummary("Create a floor")
            .Produces<FloorResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateFloor")
            .WithSummary("Replace a floor's editable state")
            .Produces<FloorResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteFloor")
            .WithSummary("Delete a floor without access points or occupancy sessions")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<FloorResponse>>> GetAllAsync(
        IFloorService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FloorDto> floors = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<FloorResponse>>(
            floors.Select(FloorResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<FloorResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IFloorService service,
        CancellationToken cancellationToken)
    {
        FloorDto? floor = await service.GetByIdAsync(id, cancellationToken);
        return floor is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(FloorResponse.FromDto(floor));
    }

    private static async Task<Results<Created<FloorResponse>, ProblemHttpResult>> CreateAsync(
        CreateFloorRequest request,
        IFloorService service,
        CancellationToken cancellationToken)
    {
        FloorSaveResult result = await service.CreateAsync(
            new CreateFloorCommand(request.BuildingId, request.Number, request.Name),
            cancellationToken);

        return result.Status switch
        {
            FloorSaveStatus.Success => CreateResponse(result.Floor!),
            FloorSaveStatus.BuildingNotFound => ParentBuildingNotFound(request.BuildingId),
            _ => throw new InvalidOperationException($"Unsupported create result: {result.Status}.")
        };
    }

    private static async Task<Results<Ok<FloorResponse>, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateFloorRequest request,
        IFloorService service,
        CancellationToken cancellationToken)
    {
        FloorSaveResult result = await service.UpdateAsync(
            id,
            new UpdateFloorCommand(
                request.BuildingId,
                request.Number,
                request.Name,
                request.IsActive),
            cancellationToken);

        return result.Status switch
        {
            FloorSaveStatus.Success => TypedResults.Ok(FloorResponse.FromDto(result.Floor!)),
            FloorSaveStatus.FloorNotFound => TypedResults.Problem(
                title: "Floor not found.",
                statusCode: StatusCodes.Status404NotFound),
            FloorSaveStatus.BuildingNotFound => ParentBuildingNotFound(request.BuildingId),
            _ => throw new InvalidOperationException($"Unsupported update result: {result.Status}.")
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IFloorService service,
        CancellationToken cancellationToken)
    {
        DeleteFloorResult result = await service.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteFloorResult.Deleted => TypedResults.NoContent(),
            DeleteFloorResult.NotFound => TypedResults.NotFound(),
            DeleteFloorResult.HasDependents => TypedResults.Problem(
                title: "Floor cannot be deleted.",
                detail: "Remove or reassign its access points and occupancy sessions before deleting the floor.",
                statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException($"Unsupported delete result: {result}.")
        };
    }

    private static Created<FloorResponse> CreateResponse(FloorDto floor)
    {
        FloorResponse response = FloorResponse.FromDto(floor);
        return TypedResults.Created($"/api/floors/{response.Id}", response);
    }

    private static ProblemHttpResult ParentBuildingNotFound(Guid buildingId)
    {
        return TypedResults.Problem(
            title: "Building not found.",
            detail: $"Building '{buildingId}' does not exist.",
            statusCode: StatusCodes.Status404NotFound);
    }
}