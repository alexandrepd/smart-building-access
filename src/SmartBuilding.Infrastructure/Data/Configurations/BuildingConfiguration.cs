using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("buildings");
        builder.HasKey(building => building.Id);

        builder.Property(building => building.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(building => building.Address)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(building => building.CreatedAt)
            .HasColumnType("timestamp with time zone");
    }
}