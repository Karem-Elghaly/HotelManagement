
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Number).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Notes).HasMaxLength(500);

        
        builder.Property(r => r.Status).HasConversion<int>();

    
        builder.HasIndex(r => r.Number)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(r => r.RoomType)         
            .WithMany(rt => rt.Rooms)            
            .HasForeignKey(r => r.RoomTypeId)   
            .OnDelete(DeleteBehavior.Restrict);  

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}