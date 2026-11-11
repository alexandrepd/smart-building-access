using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Api.Contracts.Occupancy;
using SmartBuilding.Api.Endpoints;
using SmartBuilding.Api.Hubs;
using SmartBuilding.Api.Security;
using SmartBuilding.Application.AccessControl;
using SmartBuilding.Application.Occupancy;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.UnitTests.Api.Hubs;

public class AccessHubTests
{
    [Theory]
    [InlineData(null, false, HttpStatusCode.Unauthorized)]
    [InlineData("invalid", true, HttpStatusCode.Unauthorized)]
    [InlineData("OtherRole", true, HttpStatusCode.Forbidden)]
    [InlineData("Admin", true, HttpStatusCode.OK)]
    [InlineData("SecurityOperator", true, HttpStatusCode.OK)]
    [InlineData("Admin", false, HttpStatusCode.OK)]
    [InlineData("SecurityOperator", false, HttpStatusCode.OK)]
    public async Task Negotiate_RequiresAccessOperations(
        string? role, bool queryToken, HttpStatusCode expected)
    {
        await using var host = await TestHost.CreateAsync();
        string? token = role is null ? null
            : role == "invalid" ? "invalid" : host.CreateToken(role);
        string path = "/hubs/access/negotiate?negotiateVersion=1";
        if (queryToken)
        {
            path += $"&access_token={token}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (!queryToken && token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await host.Client.SendAsync(request);

        Assert.Equal(expected, response.StatusCode);
        if (expected != HttpStatusCode.OK)
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Theory]
    [InlineData("/api/access/events")]
    [InlineData("/hubs/access-other")]
    public async Task QueryToken_IsNotAcceptedOutsideHubPath(string path)
    {
        await using var host = await TestHost.CreateAsync();
        using var response = await host.Client.GetAsync(
            $"{path}?access_token={host.CreateToken("Admin")}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(AccessDirection.Entry, "Admin", HttpTransportType.LongPolling)]
    [InlineData(AccessDirection.Exit, "SecurityOperator", HttpTransportType.LongPolling)]
    [InlineData(AccessDirection.Entry, "Admin", HttpTransportType.WebSockets)]
    [InlineData(AccessDirection.Exit, "SecurityOperator", HttpTransportType.WebSockets)]
    public async Task GrantedRequest_BroadcastsRestPayloadAndPostCommitOccupancy(
        AccessDirection direction, string role, HttpTransportType transport)
    {
        await using var host = await TestHost.CreateAsync();
        await using var connection = host.CreateConnection(role, transport);
        var eventReceived = new TaskCompletionSource<AccessEventResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var occupancyReceived = new TaskCompletionSource<CurrentOccupancyResponse[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var eventSubscription = connection.On<AccessEventResponse>(
            "AccessEventCreated", response => eventReceived.TrySetResult(response));
        using var occupancySubscription = connection.On<CurrentOccupancyResponse[]>(
            "OccupancyChanged", response => occupancyReceived.TrySetResult(response));
        await connection.StartAsync();

        using var response = await host.RequestAccessAsync(direction);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var restEvent = await response.Content.ReadFromJsonAsync<AccessEventResponse>(
            TestHost.JsonOptions);
        Assert.Equal(restEvent, await eventReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        var occupancy = await occupancyReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var floor = Assert.Single(occupancy);
        Assert.Equal(host.Occupancy.Floor.FloorId, floor.FloorId);
        Assert.Equal(host.Occupancy.Floor.BuildingId, floor.BuildingId);
        Assert.Equal(host.Occupancy.Floor.OccupantCount, floor.OccupantCount);
        var occupant = Assert.Single(floor.Occupants);
        Assert.Equal(host.Occupancy.Floor.Occupants[0].UserId, occupant.UserId);
        Assert.Equal(42, occupant.DurationSeconds);
        Assert.Equal(1, host.Occupancy.ReadCount);
    }

    [Fact]
    public async Task DeniedRequest_BroadcastsAccessEventWithoutReadingOccupancy()
    {
        await using var host = await TestHost.CreateAsync();
        host.Access.Result = AccessResult.NoPermission;
        await using var connection = host.CreateConnection("SecurityOperator");
        var eventReceived = new TaskCompletionSource<AccessEventResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = connection.On<AccessEventResponse>(
            "AccessEventCreated", response => eventReceived.TrySetResult(response));
        await connection.StartAsync();

        using var response = await host.RequestAccessAsync(AccessDirection.Entry);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AccessResult.NoPermission,
            (await eventReceived.Task.WaitAsync(TimeSpan.FromSeconds(5))).Result);
        Assert.Equal(0, host.Occupancy.ReadCount);
    }

    [Fact]
    public async Task MissingAccessPoint_ReturnsExistingProblemWithoutPublishing()
    {
        await using var host = await TestHost.CreateAsync();
        host.Access.MissingAccessPoint = true;
        using var response = await host.RequestAccessAsync(AccessDirection.Entry);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Access point not found.", problem.GetProperty("title").GetString());
        Assert.False(host.Access.Recorded);
        Assert.Equal(0, host.Occupancy.ReadCount);
    }

    [Fact]
    public async Task OccupancyReadFailure_DoesNotChangeRecordedRestResponse()
    {
        await using var host = await TestHost.CreateAsync();
        host.Occupancy.FailRead = true;
        using var response = await host.RequestAccessAsync(AccessDirection.Entry);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(host.Access.Recorded);
        Assert.Equal(1, host.Occupancy.ReadCount);
    }

    private sealed class TestHost(WebApplication app, byte[] signingKey) : IAsyncDisposable
    {
        internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
        };

        internal HttpClient Client { get; } = app.GetTestClient();
        internal AccessService Access { get; } = app.Services.GetRequiredService<AccessService>();
        internal OccupancyService Occupancy { get; } = app.Services.GetRequiredService<OccupancyService>();

        internal static async Task<TestHost> CreateAsync()
        {
            var key = RandomNumberGenerator.GetBytes(32);
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Logging.ClearProviders();
            builder.Services.AddValidation();
            builder.Services.AddProblemDetails();
            builder.Services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(allowIntegerValues: false)));
            builder.Services.AddSignalR().AddJsonProtocol(options =>
                options.PayloadSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(allowIntegerValues: false)));
            builder.Services.AddScoped<AccessEventPublisher>();
            builder.Services.AddSingleton<AccessService>();
            builder.Services.AddSingleton<IAccessControlService>(sp =>
                sp.GetRequiredService<AccessService>());
            builder.Services.AddSingleton<OccupancyService>();
            builder.Services.AddSingleton<IOccupancyService>(sp =>
                sp.GetRequiredService<OccupancyService>());
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.Events = new AccessHubJwtBearerEvents();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = "hub-tests",
                        ValidAudience = "hub-tests",
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = "role",
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                    };
                });
            builder.Services.AddAuthorization(options =>
                options.AddPolicy(AuthorizationPolicies.AccessOperations, policy =>
                    policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SecurityOperator))));
            var app = builder.Build();
            app.UseStatusCodePages();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<AccessHub>(AccessHub.Path, options =>
                options.CloseOnAuthenticationExpiration = true);
            app.MapAccessControlEndpoints();
            app.MapGet("/hubs/access-other", () => Results.Ok())
                .RequireAuthorization(AuthorizationPolicies.AccessOperations);
            await app.StartAsync();
            return new TestHost(app, key);
        }

        internal string CreateToken(string role)
        {
            var token = new JwtSecurityToken(
                issuer: "hub-tests", audience: "hub-tests",
                claims: [new Claim("role", role)],
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        internal HubConnection CreateConnection(
            string role, HttpTransportType transport = HttpTransportType.LongPolling) =>
            new HubConnectionBuilder()
                .WithUrl($"http://localhost/hubs/access?access_token={CreateToken(role)}", options =>
                {
                    options.Transports = transport;
                    options.HttpMessageHandlerFactory = _ => app.GetTestServer().CreateHandler();
                    options.WebSocketFactory = async (context, cancellationToken) =>
                        await app.GetTestServer().CreateWebSocketClient()
                            .ConnectAsync(context.Uri, cancellationToken);
                })
                .AddJsonProtocol(options =>
                    options.PayloadSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(allowIntegerValues: false)))
                .Build();

        internal Task<HttpResponseMessage> RequestAccessAsync(AccessDirection direction)
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CreateToken("Admin"));
            return Client.PostAsJsonAsync("/api/access/requests", new
            {
                CardNumber = "test-card",
                AccessPointId = Guid.NewGuid(),
                Direction = direction
            }, JsonOptions);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }

    private sealed class AccessService : IAccessControlService
    {
        internal AccessResult Result { get; set; } = AccessResult.Granted;
        internal bool MissingAccessPoint { get; set; }
        internal bool Recorded { get; private set; }

        public Task<AccessRequestResult> RequestAccessAsync(
            AccessRequestCommand command, CancellationToken cancellationToken)
        {
            if (MissingAccessPoint)
            {
                return Task.FromResult(AccessRequestResult.MissingAccessPoint());
            }

            Recorded = true;
            return Task.FromResult(AccessRequestResult.Recorded(new AccessEventDto(
                Guid.NewGuid(), command.AccessPointId, Guid.NewGuid(), Guid.NewGuid(),
                DateTime.UtcNow, command.Direction, Result, "Test decision.")));
        }

        public Task<IReadOnlyList<AccessEventDto>> GetEventsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AccessEventDto>>([]);

        public Task<AccessEventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<AccessEventDto?>(null);
    }

    private sealed class OccupancyService(AccessService access) : IOccupancyService
    {
        internal CurrentOccupancyFloorDto Floor { get; } = new(
            Guid.NewGuid(), "Building", Guid.NewGuid(), "Floor", 1,
            [new CurrentOccupantDto(Guid.NewGuid(), "Occupant", DateTime.UtcNow, 42)]);
        internal int ReadCount { get; private set; }
        internal bool FailRead { get; set; }

        public Task<IReadOnlyList<CurrentOccupancyFloorDto>> GetCurrentOccupancyAsync(
            CancellationToken cancellationToken)
        {
            Assert.True(access.Recorded);
            Assert.False(cancellationToken.IsCancellationRequested);
            ReadCount++;
            if (FailRead)
            {
                throw new InvalidOperationException("Occupancy unavailable.");
            }

            return Task.FromResult<IReadOnlyList<CurrentOccupancyFloorDto>>([Floor]);
        }
    }
}
