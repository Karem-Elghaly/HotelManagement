
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class HousekeepingTaskConfiguration : IEntityTypeConfiguration<HousekeepingTask>
{
    public void Configure(EntityTypeBuilder<HousekeepingTask> builder)
    {
        builder.ToTable("HousekeepingTasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type).HasConversion<int>();
        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.Notes).HasMaxLength(500);

     
        builder.Property(t => t.AssignedToUserId).HasMaxLength(450);
        builder.Property(t => t.CreatedByUserId).HasMaxLength(450);

       
        builder.Property(t => t.RowVersion).IsRowVersion();

      
        builder.HasOne(t => t.Room)
            .WithMany()
            .HasForeignKey(t => t.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

      
        builder.HasIndex(t => new { t.Status, t.RoomId });

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}