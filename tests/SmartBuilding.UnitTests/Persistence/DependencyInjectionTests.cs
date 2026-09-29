using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Infrastructure;
using SmartBuilding.Infrastructure.Data;

namespace SmartBuilding.UnitTests.Persistence;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ValidConnectionString_RegistersScopedPostgreSqlContext()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(
            "Host=localhost;Database=dependency_injection;Username=unused");

        using ServiceProvider serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope firstScope = serviceProvider.CreateScope();
        using IServiceScope secondScope = serviceProvider.CreateScope();

        SmartBuildingDbContext firstContext =
            firstScope.ServiceProvider.GetRequiredService<SmartBuildingDbContext>();
        SmartBuildingDbContext sameScopeContext =
            firstScope.ServiceProvider.GetRequiredService<SmartBuildingDbContext>();
        SmartBuildingDbContext secondContext =
            secondScope.ServiceProvider.GetRequiredService<SmartBuildingDbContext>();

        Assert.Same(firstContext, sameScopeContext);
        Assert.NotSame(firstContext, secondContext);
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", firstContext.Database.ProviderName);

        IEntityType accessEvent = Assert.IsAssignableFrom<IEntityType>(
            firstContext.Model.FindEntityType(typeof(AccessEvent)));
        StoreObjectIdentifier table = StoreObjectIdentifier.Table(
            Assert.IsType<string>(accessEvent.GetTableName()),
            accessEvent.GetSchema());

        Assert.Equal(
            "access_point_id",
            accessEvent.FindProperty(nameof(AccessEvent.AccessPointId))?.GetColumnName(table));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInfrastructure_MissingConnectionString_ThrowsArgumentException(
        string connectionString)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() =>
            services.AddInfrastructure(connectionString));
    }
}