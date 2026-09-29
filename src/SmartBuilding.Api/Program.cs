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
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    Name = "Smart Building API",
    Status = "Running"
}));

app.Run();
