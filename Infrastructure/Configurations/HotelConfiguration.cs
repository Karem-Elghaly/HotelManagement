
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;


public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
   
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        
        builder.ToTable("Hotels");

       
        builder.HasKey(h => h.Id);

      
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);

     
        builder.Property(h => h.Description).HasMaxLength(2000);

        builder.Property(h => h.Address).IsRequired().HasMaxLength(300);
        builder.Property(h => h.City).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Phone).IsRequired().HasMaxLength(30);
        builder.Property(h => h.Email).IsRequired().HasMaxLength(150);

     
        builder.HasQueryFilter(h => !h.IsDeleted);

 
        builder.HasData(new Hotel
        {
            Id = 1,
            Name = "Grand Hotel",
            Description = "Our flagship hotel.",
            Address = "1 Nile Street",
            City = "Cairo",
            Phone = "+20000000000",
            Email = "info@grandhotel.com",
            StarRating = 5,
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            FreeCancellationHours = 48,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}