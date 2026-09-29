using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Infrastructure.Data;

namespace SmartBuilding.UnitTests.Persistence;

public class ModelMetadataTests
{
    private readonly IModel model;

    public ModelMetadataTests()
    {
        DbContextOptions<SmartBuildingDbContext> options =
            new DbContextOptionsBuilder<SmartBuildingDbContext>()
                .UseNpgsql("Host=localhost;Database=model_metadata;Username=unused;Password=unused")
                .UseSnakeCaseNamingConvention()
                .Options;

        using var context = new SmartBuildingDbContext(options);
        model = context.Model;
    }

    [Fact]
    public void Model_ExpectedEntities_UsesExpectedTableNames()
    {
        Dictionary<Type, string> expectedTables = new()
        {
            [typeof(Building)] = "buildings",
            [typeof(Floor)] = "floors",
            [typeof(AccessPoint)] = "access_points",
            [typeof(User)] = "users",
            [typeof(AccessCard)] = "access_cards",
            [typeof(AccessPermission)] = "access_permissions",
            [typeof(AccessEvent)] = "access_events",
            [typeof(OccupancySession)] = "occupancy_sessions",
            [typeof(SecurityAlert)] = "security_alerts"
        };

        foreach ((Type entityType, string tableName) in expectedTables)
        {
            Assert.Equal(tableName, model.FindEntityType(entityType)?.GetTableName());
        }
    }

    [Theory]
    [InlineData(typeof(User), nameof(User.Email))]
    [InlineData(typeof(AccessCard), nameof(AccessCard.CardNumber))]
    public void Model_UniqueBusinessKeys_HasUniqueIndex(Type entityType, string propertyName)
    {
        IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(model.FindEntityType(entityType));

        Assert.Contains(
            metadata.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([propertyName]));
    }

    [Theory]
    [InlineData(typeof(Floor), nameof(Floor.BuildingId), false)]
    [InlineData(typeof(AccessPoint), nameof(AccessPoint.FloorId), false)]
    [InlineData(typeof(AccessCard), nameof(AccessCard.UserId), false)]
    [InlineData(typeof(AccessPermission), nameof(AccessPermission.UserId), false)]
    [InlineData(typeof(AccessPermission), nameof(AccessPermission.AccessPointId), false)]
    [InlineData(typeof(AccessEvent), nameof(AccessEvent.AccessPointId), false)]
    [InlineData(typeof(AccessEvent), nameof(AccessEvent.AccessCardId), true)]
    [InlineData(typeof(AccessEvent), nameof(AccessEvent.UserId), true)]
    [InlineData(typeof(OccupancySession), nameof(OccupancySession.UserId), false)]
    [InlineData(typeof(OccupancySession), nameof(OccupancySession.FloorId), false)]
    [InlineData(typeof(SecurityAlert), nameof(SecurityAlert.AccessPointId), false)]
    public void Model_Relationships_HasExpectedOptionality(
        Type entityType,
        string foreignKeyProperty,
        bool isOptional)
    {
        IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(model.FindEntityType(entityType));
        IForeignKey foreignKey = Assert.Single(
            metadata.GetForeignKeys(),
            candidate => candidate.Properties.Any(property => property.Name == foreignKeyProperty));

        Assert.Equal(isOptional, !foreignKey.IsRequired);
    }
}