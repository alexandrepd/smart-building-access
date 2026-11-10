using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.AccessCards;
using SmartBuilding.Api.Security;
using SmartBuilding.Application.AccessCards;

namespace SmartBuilding.Api.Endpoints;

internal static class AccessCardEndpoints
{
    internal static IEndpointRouteBuilder MapAccessCardEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/cards")
            .WithTags("Access Cards")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", GetAllAsync)
            .WithName("GetAccessCards")
            .WithSummary("List access cards")
            .WithDescription("Returns cards ordered by card number.")
            .Produces<IReadOnlyList<AccessCardResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetAccessCardById")
            .WithSummary("Get an access card by ID")
            .Produces<AccessCardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateAccessCard")
            .WithSummary("Issue an access card")
            .Produces<AccessCardResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateAccessCard")
            .WithSummary("Replace an access card's assignment and details")
            .Produces<AccessCardResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateAccessCard")
            .WithSummary("Activate an access card")
            .Produces<AccessCardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateAccessCard")
            .WithSummary("Deactivate an access card")
            .Produces<AccessCardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<AccessCardResponse>>> GetAllAsync(
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessCardDto> accessCards =
            await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<AccessCardResponse>>(
            accessCards.Select(AccessCardResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<AccessCardResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        AccessCardDto? accessCard = await service.GetByIdAsync(id, cancellationToken);
        return accessCard is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(AccessCardResponse.FromDto(accessCard));
    }

    private static async Task<Results<Created<AccessCardResponse>, ProblemHttpResult>> CreateAsync(
        CreateAccessCardRequest request,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        AccessCardSaveResult result = await service.CreateAsync(
            new CreateAccessCardCommand(request.UserId, request.CardNumber, request.ExpiresAt),
            cancellationToken);

        return result.Status switch
        {
            AccessCardSaveStatus.Success => CreateResponse(result.AccessCard!),
            AccessCardSaveStatus.UserNotFound => UserNotFound(),
            AccessCardSaveStatus.CardNumberAlreadyExists => DuplicateCardNumber(),
            _ => throw new InvalidOperationException(
                $"Unsupported access card create result: {result.Status}.")
        };
    }

    private static async Task<Results<Ok<AccessCardResponse>, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateAccessCardRequest request,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        AccessCardSaveResult result = await service.UpdateAsync(
            id,
            new UpdateAccessCardCommand(request.UserId, request.CardNumber, request.ExpiresAt),
            cancellationToken);

        return result.Status switch
        {
            AccessCardSaveStatus.Success =>
                TypedResults.Ok(AccessCardResponse.FromDto(result.AccessCard!)),
            AccessCardSaveStatus.AccessCardNotFound => AccessCardNotFound(),
            AccessCardSaveStatus.UserNotFound => UserNotFound(),
            AccessCardSaveStatus.CardNumberAlreadyExists => DuplicateCardNumber(),
            _ => throw new InvalidOperationException(
                $"Unsupported access card update result: {result.Status}.")
        };
    }

    private static Task<Results<Ok<AccessCardResponse>, NotFound>> ActivateAsync(
        Guid id,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        return SetActiveAsync(id, true, service, cancellationToken);
    }

    private static Task<Results<Ok<AccessCardResponse>, NotFound>> DeactivateAsync(
        Guid id,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        return SetActiveAsync(id, false, service, cancellationToken);
    }

    private static async Task<Results<Ok<AccessCardResponse>, NotFound>> SetActiveAsync(
        Guid id,
        bool isActive,
        IAccessCardService service,
        CancellationToken cancellationToken)
    {
        AccessCardSaveResult result = await service.SetActiveAsync(
            id,
            isActive,
            cancellationToken);

        return result.Status switch
        {
            AccessCardSaveStatus.Success =>
                TypedResults.Ok(AccessCardResponse.FromDto(result.AccessCard!)),
            AccessCardSaveStatus.AccessCardNotFound => TypedResults.NotFound(),
            _ => throw new InvalidOperationException(
                $"Unsupported access card state change result: {result.Status}.")
        };
    }

    private static Created<AccessCardResponse> CreateResponse(AccessCardDto accessCard)
    {
        AccessCardResponse response = AccessCardResponse.FromDto(accessCard);
        return TypedResults.Created($"/api/cards/{response.Id}", response);
    }

    private static ProblemHttpResult AccessCardNotFound()
    {
        return TypedResults.Problem(
            title: "Access card not found.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private static ProblemHttpResult UserNotFound()
    {
        return TypedResults.Problem(
            title: "User not found.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private static ProblemHttpResult DuplicateCardNumber()
    {
        return TypedResults.Problem(
            title: "Card number already exists.",
            statusCode: StatusCodes.Status409Conflict);
    }
}
