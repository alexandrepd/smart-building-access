using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class AccessPointConfiguration : IEntityTypeConfiguration<AccessPoint>
{
    public void Configure(EntityTypeBuilder<AccessPoint> builder)
    {
        builder.ToTable("access_points");
        builder.HasKey(accessPoint => accessPoint.Id);

        builder.Property(accessPoint => accessPoint.Name)
            .HasMaxLength(150)
            .IsRequired();
        builder.Property(accessPoint => accessPoint.Location)
            .HasMaxLength(250)
            .IsRequired();
        builder.Property(accessPoint => accessPoint.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(accessPoint => accessPoint.FloorId);
        builder.HasOne(accessPoint => accessPoint.Floor)
            .WithMany(floor => floor.AccessPoints)
            .HasForeignKey(accessPoint => accessPoint.FloorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}