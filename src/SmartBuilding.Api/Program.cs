using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartBuilding.Api.Configuration;
using SmartBuilding.Api.Endpoints;
using SmartBuilding.Api.ErrorHandling;
using SmartBuilding.Api.Security;
using SmartBuilding.Application;
using SmartBuilding.Application.Authentication;
using SmartBuilding.Domain.Enums;
using SmartBuilding.Infrastructure;
using SmartBuilding.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
JwtSettings jwtSettings = JwtSettings.FromConfiguration(builder.Configuration);

string? connectionString = builder.Configuration.GetConnectionString("SmartBuilding");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'SmartBuilding' is required. Configure it with user secrets or environment variables.");
}

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role",
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(nameof(UserRole.Admin)));
    options.AddPolicy(AuthorizationPolicies.AccessOperations, policy =>
        policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SecurityOperator)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

await app.Services.EnsureBootstrapAdminAsync(
    builder.Configuration["Authentication:BootstrapAdmin:Email"],
    builder.Configuration["Authentication:BootstrapAdmin:Password"]);

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    Name = "Smart Building API",
    Status = "Running"
})).AllowAnonymous();

app.MapAuthenticationEndpoints();
app.MapBuildingEndpoints();
app.MapFloorEndpoints();
app.MapAccessPointEndpoints();
app.MapAccessCardEndpoints();
app.MapAccessPermissionEndpoints();
app.MapAccessControlEndpoints();
app.MapUserEndpoints();

app.Run();
