using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class AccessCardConfiguration : IEntityTypeConfiguration<AccessCard>
{
    public void Configure(EntityTypeBuilder<AccessCard> builder)
    {
        builder.ToTable("access_cards");
        builder.HasKey(accessCard => accessCard.Id);

        builder.Property(accessCard => accessCard.CardNumber)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(accessCard => accessCard.ExpiresAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(accessCard => accessCard.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(accessCard => accessCard.CardNumber)
            .IsUnique();
        builder.HasIndex(accessCard => accessCard.UserId);
        builder.HasOne(accessCard => accessCard.User)
            .WithMany(user => user.AccessCards)
            .HasForeignKey(accessCard => accessCard.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}