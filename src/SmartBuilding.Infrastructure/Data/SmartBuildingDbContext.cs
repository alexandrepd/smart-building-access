using Microsoft.EntityFrameworkCore;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data;

public class SmartBuildingDbContext(DbContextOptions<SmartBuildingDbContext> options)
    : DbContext(options)
{
    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<Floor> Floors => Set<Floor>();

    public DbSet<AccessPoint> AccessPoints => Set<AccessPoint>();

    public DbSet<User> Users => Set<User>();

    public DbSet<AccessCard> AccessCards => Set<AccessCard>();

    public DbSet<AccessPermission> AccessPermissions => Set<AccessPermission>();

    public DbSet<AccessEvent> AccessEvents => Set<AccessEvent>();

    public DbSet<OccupancySession> OccupancySessions => Set<OccupancySession>();

    public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartBuildingDbContext).Assembly);
    }
}