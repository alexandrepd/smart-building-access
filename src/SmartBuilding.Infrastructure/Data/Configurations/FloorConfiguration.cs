using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("floors");
        builder.HasKey(floor => floor.Id);

        builder.Property(floor => floor.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(floor => floor.BuildingId);
        builder.HasOne(floor => floor.Building)
            .WithMany(building => building.Floors)
            .HasForeignKey(floor => floor.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}