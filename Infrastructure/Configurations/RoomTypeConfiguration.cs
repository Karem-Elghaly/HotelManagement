
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name).IsRequired().HasMaxLength(100);
        builder.Property(rt => rt.Description).HasMaxLength(1000);

   
        builder.Property(rt => rt.BasePricePerNight).HasPrecision(18, 2);

      
        builder.HasIndex(rt => rt.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(rt => !rt.IsDeleted);
    }
}