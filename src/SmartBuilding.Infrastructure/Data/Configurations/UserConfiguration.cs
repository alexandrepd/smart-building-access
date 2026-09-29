using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();
        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(user => user.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(user => user.Email)
            .IsUnique();
    }
}