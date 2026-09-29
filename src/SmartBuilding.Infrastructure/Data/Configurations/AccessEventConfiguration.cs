using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Configurations;

public class AccessEventConfiguration : IEntityTypeConfiguration<AccessEvent>
{
    public void Configure(EntityTypeBuilder<AccessEvent> builder)
    {
        builder.ToTable("access_events");
        builder.HasKey(accessEvent => accessEvent.Id);

        builder.Property(accessEvent => accessEvent.OccurredAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(accessEvent => accessEvent.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(accessEvent => accessEvent.OccurredAt);
        builder.HasIndex(accessEvent => accessEvent.AccessPointId);
        builder.HasIndex(accessEvent => accessEvent.AccessCardId);
        builder.HasIndex(accessEvent => accessEvent.UserId);

        builder.HasOne(accessEvent => accessEvent.AccessPoint)
            .WithMany(accessPoint => accessPoint.AccessEvents)
            .HasForeignKey(accessEvent => accessEvent.AccessPointId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(accessEvent => accessEvent.AccessCard)
            .WithMany(accessCard => accessCard.AccessEvents)
            .HasForeignKey(accessEvent => accessEvent.AccessCardId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(accessEvent => accessEvent.User)
            .WithMany(user => user.AccessEvents)
            .HasForeignKey(accessEvent => accessEvent.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}