using SmartBuilding.Api.Endpoints;
using SmartBuilding.Api.ErrorHandling;
using SmartBuilding.Application;
using SmartBuilding.Infrastructure;
using SmartBuilding.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("SmartBuilding");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'SmartBuilding' is required. Configure it with user secrets or environment variables.");
}

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/", () => Results.Ok(new
{
    Name = "Smart Building API",
    Status = "Running"
}));

app.MapBuildingEndpoints();
app.MapFloorEndpoints();
app.MapAccessPointEndpoints();

app.Run();
