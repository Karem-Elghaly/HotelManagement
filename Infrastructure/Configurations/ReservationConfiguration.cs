
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
       
        builder.ToTable("Reservations", t =>
            t.HasCheckConstraint("CK_Reservations_Dates", "[CheckOutDate] > [CheckInDate]"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ConfirmationCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(r => r.ConfirmationCode).IsUnique();

        builder.Property(r => r.TotalPrice).HasPrecision(18, 2);
        builder.Property(r => r.SpecialRequests).HasMaxLength(500);
        builder.Property(r => r.Status).HasConversion<int>();

      
        builder.HasOne(r => r.Room)
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        
        builder.HasIndex(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate });

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}