using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class AccessPermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        builder.ToTable("access_permissions");
        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.ValidFrom)
            .HasColumnType("timestamp with time zone");
        builder.Property(permission => permission.ValidUntil)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(permission => permission.UserId);
        builder.HasIndex(permission => permission.AccessPointId);
        builder.HasOne(permission => permission.User)
            .WithMany(user => user.AccessPermissions)
            .HasForeignKey(permission => permission.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(permission => permission.AccessPoint)
            .WithMany(accessPoint => accessPoint.AccessPermissions)
            .HasForeignKey(permission => permission.AccessPointId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}