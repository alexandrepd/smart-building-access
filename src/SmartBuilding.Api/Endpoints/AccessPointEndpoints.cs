using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.AccessPoints;
using SmartBuilding.Application.AccessPoints;

namespace SmartBuilding.Api.Endpoints;

internal static class AccessPointEndpoints
{
    internal static IEndpointRouteBuilder MapAccessPointEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/access-points")
            .WithTags("Access Points");

        group.MapGet("/", GetAllAsync)
            .WithName("GetAccessPoints")
            .WithSummary("List access points")
            .Produces<IReadOnlyList<AccessPointResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetAccessPointById")
            .WithSummary("Get an access point by ID")
            .Produces<AccessPointResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateAccessPoint")
            .WithSummary("Create an access point on a floor")
            .Produces<AccessPointResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateAccessPoint")
            .WithSummary("Replace an access point's editable state")
            .Produces<AccessPointResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteAccessPoint")
            .WithSummary("Delete an access point without dependent history or permissions")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<AccessPointResponse>>> GetAllAsync(
        IAccessPointService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessPointDto> accessPoints = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<AccessPointResponse>>(
            accessPoints.Select(AccessPointResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<AccessPointResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IAccessPointService service,
        CancellationToken cancellationToken)
    {
        AccessPointDto? accessPoint = await service.GetByIdAsync(id, cancellationToken);
        return accessPoint is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(AccessPointResponse.FromDto(accessPoint));
    }

    private static async Task<Results<Created<AccessPointResponse>, ProblemHttpResult>> CreateAsync(
        CreateAccessPointRequest request,
        IAccessPointService service,
        CancellationToken cancellationToken)
    {
        AccessPointSaveResult result = await service.CreateAsync(
            new CreateAccessPointCommand(
                request.FloorId,
                request.Name,
                request.Location,
                request.SupportsEntry,
                request.SupportsExit),
            cancellationToken);

        return result.Status switch
        {
            AccessPointSaveStatus.Success => CreateResponse(result.AccessPoint!),
            AccessPointSaveStatus.FloorNotFound => ParentFloorNotFound(request.FloorId),
            _ => throw new InvalidOperationException(
                $"Unsupported access point create result: {result.Status}.")
        };
    }

    private static async Task<Results<Ok<AccessPointResponse>, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateAccessPointRequest request,
        IAccessPointService service,
        CancellationToken cancellationToken)
    {
        AccessPointSaveResult result = await service.UpdateAsync(
            id,
            new UpdateAccessPointCommand(
                request.FloorId,
                request.Name,
                request.Location,
                request.IsActive,
                request.SupportsEntry,
                request.SupportsExit),
            cancellationToken);

        return result.Status switch
        {
            AccessPointSaveStatus.Success =>
                TypedResults.Ok(AccessPointResponse.FromDto(result.AccessPoint!)),
            AccessPointSaveStatus.AccessPointNotFound => TypedResults.Problem(
                title: "Access point not found.",
                statusCode: StatusCodes.Status404NotFound),
            AccessPointSaveStatus.FloorNotFound => ParentFloorNotFound(request.FloorId),
            _ => throw new InvalidOperationException(
                $"Unsupported access point update result: {result.Status}.")
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IAccessPointService service,
        CancellationToken cancellationToken)
    {
        DeleteAccessPointResult result = await service.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteAccessPointResult.Deleted => TypedResults.NoContent(),
            DeleteAccessPointResult.NotFound => TypedResults.NotFound(),
            DeleteAccessPointResult.HasDependents => TypedResults.Problem(
                title: "Access point cannot be deleted.",
                detail: "Access permissions, access events, or security alerts reference this access point.",
                statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException(
                $"Unsupported access point delete result: {result}.")
        };
    }

    private static Created<AccessPointResponse> CreateResponse(AccessPointDto accessPoint)
    {
        AccessPointResponse response = AccessPointResponse.FromDto(accessPoint);
        return TypedResults.Created($"/api/access-points/{response.Id}", response);
    }

    private static ProblemHttpResult ParentFloorNotFound(Guid floorId)
    {
        return TypedResults.Problem(
            title: "Floor not found.",
            detail: $"Floor '{floorId}' does not exist.",
            statusCode: StatusCodes.Status404NotFound);
    }
}