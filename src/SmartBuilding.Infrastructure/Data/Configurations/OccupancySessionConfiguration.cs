using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class OccupancySessionConfiguration : IEntityTypeConfiguration<OccupancySession>
{
    public void Configure(EntityTypeBuilder<OccupancySession> builder)
    {
        builder.ToTable("occupancy_sessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.EnteredAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(session => session.ExitedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(session => session.UserId);
        builder.HasIndex(session => session.FloorId);
        builder.HasIndex(session => session.ExitedAt);
        builder.HasOne(session => session.User)
            .WithMany(user => user.OccupancySessions)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(session => session.Floor)
            .WithMany(floor => floor.OccupancySessions)
            .HasForeignKey(session => session.FloorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}