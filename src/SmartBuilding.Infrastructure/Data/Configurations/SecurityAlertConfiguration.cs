using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class SecurityAlertConfiguration : IEntityTypeConfiguration<SecurityAlert>
{
    public void Configure(EntityTypeBuilder<SecurityAlert> builder)
    {
        builder.ToTable("security_alerts");
        builder.HasKey(alert => alert.Id);

        builder.Property(alert => alert.Message)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(alert => alert.CreatedAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(alert => alert.ResolvedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(alert => alert.AccessPointId);
        builder.HasOne(alert => alert.AccessPoint)
            .WithMany(accessPoint => accessPoint.SecurityAlerts)
            .HasForeignKey(alert => alert.AccessPointId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}