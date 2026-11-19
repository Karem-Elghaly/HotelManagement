
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
    
        builder.ToTable("MaintenanceRequests", t =>
            t.HasCheckConstraint("CK_Maintenance_BlockDates",
                "[BlocksRoom] = 0 OR ([BlockedFrom] IS NOT NULL AND [BlockedUntil] IS NOT NULL AND [BlockedUntil] > [BlockedFrom])"));

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.ResolutionNotes).HasMaxLength(1000);
        builder.Property(m => m.ReportedByUserId).HasMaxLength(450);
        builder.Property(m => m.Priority).HasConversion<int>();
        builder.Property(m => m.Status).HasConversion<int>();

        builder.HasOne(m => m.Room)
            .WithMany()
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.RoomId, m.BlockedFrom, m.BlockedUntil });
        builder.HasIndex(m => m.Status);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}