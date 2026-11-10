using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartBuilding.Infrastructure.Data;

public sealed class SmartBuildingDbContextFactory
    : IDesignTimeDbContextFactory<SmartBuildingDbContext>
{
    public SmartBuildingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SmartBuildingDbContext>()
            .UseNpgsql((string?)null)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SmartBuildingDbContext(options);
    }
}
