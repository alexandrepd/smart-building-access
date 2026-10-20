using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Application.AccessControl;

namespace SmartBuilding.Api.Endpoints;

internal static class AccessControlEndpoints
{
    internal static IEndpointRouteBuilder MapAccessControlEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/access")
            .WithTags("Access Control");

        group.MapPost("/requests", RequestAccessAsync)
            .WithName("RequestAccess")
            .WithSummary("Evaluate an access request and record the decision")
            .WithDescription(
                "Returns HTTP 200 for a processed request; inspect the result field to determine whether access was granted or denied. Every processed request is audited.")
            .Produces<AccessEventResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/events", GetEventsAsync)
            .WithName("GetAccessEvents")
            .WithSummary("List access decision events")
            .WithDescription("Returns access attempts ordered newest first.")
            .Produces<IReadOnlyList<AccessEventResponse>>(StatusCodes.Status200OK);

        group.MapGet("/events/{id:guid}", GetEventByIdAsync)
            .WithName("GetAccessEventById")
            .WithSummary("Get an access decision event by ID")
            .Produces<AccessEventResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Results<Ok<AccessEventResponse>, ProblemHttpResult>>
        RequestAccessAsync(
            AccessRequestRequest request,
            IAccessControlService service,
            CancellationToken cancellationToken)
    {
        AccessRequestResult result = await service.RequestAccessAsync(
            new AccessRequestCommand(
                request.CardNumber,
                request.AccessPointId,
                request.Direction),
            cancellationToken);

        return result.Status switch
        {
            AccessDecisionStatus.Recorded =>
                TypedResults.Ok(AccessEventResponse.FromDto(result.AccessEvent!)),
            AccessDecisionStatus.AccessPointNotFound => TypedResults.Problem(
                title: "Access point not found.",
                statusCode: StatusCodes.Status404NotFound),
            _ => throw new InvalidOperationException(
                $"Unsupported access decision status: {result.Status}.")
        };
    }

    private static async Task<Ok<IReadOnlyList<AccessEventResponse>>> GetEventsAsync(
        IAccessControlService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessEventDto> events =
            await service.GetEventsAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<AccessEventResponse>>(
            events.Select(AccessEventResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<AccessEventResponse>, NotFound>> GetEventByIdAsync(
        Guid id,
        IAccessControlService service,
        CancellationToken cancellationToken)
    {
        AccessEventDto? accessEvent = await service.GetEventByIdAsync(id, cancellationToken);
        return accessEvent is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(AccessEventResponse.FromDto(accessEvent));
    }
}
