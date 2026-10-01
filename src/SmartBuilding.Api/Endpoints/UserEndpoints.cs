using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.Users;
using SmartBuilding.Application.Users;

namespace SmartBuilding.Api.Endpoints;

internal static class UserEndpoints
{
    internal static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", GetAllAsync)
            .WithName("GetUsers")
            .WithSummary("List users")
            .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get a user by ID")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateUser")
            .WithSummary("Create a user")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateUser")
            .WithSummary("Replace a user's profile and active state")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete a user without cards, permissions, or occupancy sessions")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<UserResponse>>> GetAllAsync(
        IUserService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserDto> users = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok<IReadOnlyList<UserResponse>>(
            users.Select(UserResponse.FromDto).ToArray());
    }

    private static async Task<Results<Ok<UserResponse>, NotFound>> GetByIdAsync(
        Guid id,
        IUserService service,
        CancellationToken cancellationToken)
    {
        UserDto? user = await service.GetByIdAsync(id, cancellationToken);
        return user is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(UserResponse.FromDto(user));
    }

    private static async Task<Results<Created<UserResponse>, ProblemHttpResult>> CreateAsync(
        CreateUserRequest request,
        IUserService service,
        CancellationToken cancellationToken)
    {
        UserSaveResult result = await service.CreateAsync(
            new CreateUserCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return result.Status switch
        {
            UserSaveStatus.Success => CreateResponse(result.User!),
            UserSaveStatus.EmailAlreadyExists => DuplicateEmail(request.Email),
            _ => throw new InvalidOperationException($"Unsupported user create result: {result.Status}.")
        };
    }

    private static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        IUserService service,
        CancellationToken cancellationToken)
    {
        UserSaveResult result = await service.UpdateAsync(
            id,
            new UpdateUserCommand(request.Name, request.Email, request.IsActive),
            cancellationToken);

        return result.Status switch
        {
            UserSaveStatus.Success => TypedResults.Ok(UserResponse.FromDto(result.User!)),
            UserSaveStatus.UserNotFound => TypedResults.Problem(
                title: "User not found.",
                statusCode: StatusCodes.Status404NotFound),
            UserSaveStatus.EmailAlreadyExists => DuplicateEmail(request.Email),
            _ => throw new InvalidOperationException($"Unsupported user update result: {result.Status}.")
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IUserService service,
        CancellationToken cancellationToken)
    {
        DeleteUserResult result = await service.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteUserResult.Deleted => TypedResults.NoContent(),
            DeleteUserResult.NotFound => TypedResults.NotFound(),
            DeleteUserResult.HasDependents => TypedResults.Problem(
                title: "User cannot be deleted.",
                detail: "Remove or reassign the user's cards, permissions, and occupancy sessions first.",
                statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException($"Unsupported user delete result: {result}.")
        };
    }

    private static Created<UserResponse> CreateResponse(UserDto user)
    {
        UserResponse response = UserResponse.FromDto(user);
        return TypedResults.Created($"/api/users/{response.Id}", response);
    }

    private static ProblemHttpResult DuplicateEmail(string email)
    {
        return TypedResults.Problem(
            title: "Email already exists.",
            detail: $"A user with email '{email}' already exists.",
            statusCode: StatusCodes.Status409Conflict);
    }
}