using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Api.ErrorHandling;
using SmartBuilding.Application.Common;

namespace SmartBuilding.UnitTests.Api.ErrorHandling;

public class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData("""{"cardNumber":"TEST-001","accessPointId":"30000000-0000-0000-0000-000000000001","direction":"Sideways"}""")]
    [InlineData("""{"cardNumber":"TEST-001","accessPointId":""")]
    public async Task Request_InvalidJson_ReturnsBadRequestProblem(string body)
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/requests", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("Invalid request.", problem.GetProperty("title").GetString());
        Assert.False(problem.TryGetProperty("detail", out _));
    }

    [Theory]
    [InlineData(413)]
    [InlineData(415)]
    public async Task Request_ClientTransportError_PreservesStatusCode(int statusCode)
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var response = await client.GetAsync($"/errors/http/{statusCode}");

        Assert.Equal(statusCode, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(statusCode, problem.GetProperty("status").GetInt32());
        Assert.Equal("Invalid request.", problem.GetProperty("title").GetString());
        Assert.False(problem.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task Request_ApplicationValidationFailure_PreservesFieldErrors()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var response = await client.GetAsync("/errors/validation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Invalid request.", problem.GetProperty("title").GetString());
        Assert.Equal(
            "Card number is required.",
            problem.GetProperty("errors").GetProperty("CardNumber")[0].GetString());
    }

    [Theory]
    [InlineData("/errors/server")]
    [InlineData("/errors/http/503")]
    public async Task Request_UnexpectedFailure_ReturnsGenericInternalServerError(string path)
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
        Assert.Equal("An unexpected error occurred.", problem.GetProperty("title").GetString());
        Assert.False(problem.TryGetProperty("detail", out _));
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.Configure<RouteHandlerOptions>(options =>
            options.ThrowOnBadRequest = true);
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)));

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.MapPost("/requests", (AccessRequestRequest request) =>
            TypedResults.Ok(request.Direction));
        app.MapGet("/errors/http/{statusCode:int}", IResult (int statusCode) =>
            throw new BadHttpRequestException("Internal transport details.", statusCode));
        app.MapGet("/errors/validation", IResult () =>
            throw new ApplicationValidationException(
                nameof(AccessRequestRequest.CardNumber), "Card number is required."));
        app.MapGet("/errors/server", IResult () =>
            throw new InvalidOperationException("Internal database details."));
        await app.StartAsync();
        return app;
    }
}
