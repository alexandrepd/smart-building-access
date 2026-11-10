using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.AccessPermissions;
using SmartBuilding.Api.Security;
using SmartBuilding.Application.AccessPermissions;

namespace SmartBuilding.Api.Endpoints;

internal static class AccessPermissionEndpoints
{
    internal static IEndpointRouteBuilder MapAccessPermissionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/permissions")
            .WithTags("Access Permissions")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", GetAllAsync)
            .WithName("GetAccessPermissions")
            .WithSummary("List access permissions")
            .WithDescription("Returns permissions ordered by user, access point, and validity start.")
            .Produces<IReadOnlyList<AccessPermissionResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetAccessPermissionById")
            .WithSummary("Get an access permission by ID")
            .Produces<AccessPermissionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateAccessPermission")
            .WithSummary("Grant access to an access point")
            .Produces<AccessPermissionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateAccessPermission")
            .WithSummary("Replace an access permission")
            .Produces<AccessPermissionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteAccessPermission")
            .WithSummary("Revoke an access permission")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<AccessPermissionResponse>>> GetAllAsync(
        IAccessPermissionService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessPermissionDto> permissions =
            await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<AccessPermissionResponse>>(
            permissions.Select(AccessPermissionResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<AccessPermissionResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IAccessPermissionService service,
        CancellationToken cancellationToken)
    {
        AccessPermissionDto? permission = await service.GetByIdAsync(id, cancellationToken);
        return permission is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(AccessPermissionResponse.FromDto(permission));
    }

    private static async Task<Results<Created<AccessPermissionResponse>, ProblemHttpResult>>
        CreateAsync(
            CreateAccessPermissionRequest request,
            IAccessPermissionService service,
            CancellationToken cancellationToken)
    {
        AccessPermissionSaveResult result = await service.CreateAsync(
            new CreateAccessPermissionCommand(
                request.UserId,
                request.AccessPointId,
                request.ValidFrom,
                request.ValidUntil),
            cancellationToken);

        return result.Status switch
        {
            AccessPermissionSaveStatus.Success => CreateResponse(result.Permission!),
            AccessPermissionSaveStatus.UserNotFound => UserNotFound(),
            AccessPermissionSaveStatus.AccessPointNotFound => AccessPointNotFound(),
            _ => throw new InvalidOperationException(
                $"Unsupported access permission create result: {result.Status}.")
        };
    }

    private static async Task<Results<Ok<AccessPermissionResponse>, ProblemHttpResult>>
        UpdateAsync(
            Guid id,
            UpdateAccessPermissionRequest request,
            IAccessPermissionService service,
            CancellationToken cancellationToken)
    {
        AccessPermissionSaveResult result = await service.UpdateAsync(
            id,
            new UpdateAccessPermissionCommand(
                request.UserId,
                request.AccessPointId,
                request.ValidFrom,
                request.ValidUntil,
                request.IsActive),
            cancellationToken);

        return result.Status switch
        {
            AccessPermissionSaveStatus.Success =>
                TypedResults.Ok(AccessPermissionResponse.FromDto(result.Permission!)),
            AccessPermissionSaveStatus.AccessPermissionNotFound => PermissionNotFound(),
            AccessPermissionSaveStatus.UserNotFound => UserNotFound(),
            AccessPermissionSaveStatus.AccessPointNotFound => AccessPointNotFound(),
            _ => throw new InvalidOperationException(
                $"Unsupported access permission update result: {result.Status}.")
        };
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        Guid id,
        IAccessPermissionService service,
        CancellationToken cancellationToken)
    {
        DeleteAccessPermissionResult result =
            await service.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteAccessPermissionResult.Deleted => TypedResults.NoContent(),
            DeleteAccessPermissionResult.NotFound => TypedResults.NotFound(),
            _ => throw new InvalidOperationException(
                $"Unsupported access permission delete result: {result}.")
        };
    }

    private static Created<AccessPermissionResponse> CreateResponse(
        AccessPermissionDto permission)
    {
        AccessPermissionResponse response = AccessPermissionResponse.FromDto(permission);
        return TypedResults.Created($"/api/permissions/{response.Id}", response);
    }

    private static ProblemHttpResult PermissionNotFound()
    {
        return TypedResults.Problem(
            title: "Access permission not found.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private static ProblemHttpResult UserNotFound()
    {
        return TypedResults.Problem(
            title: "User not found.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private static ProblemHttpResult AccessPointNotFound()
    {
        return TypedResults.Problem(
            title: "Access point not found.",
            statusCode: StatusCodes.Status404NotFound);
    }
}
