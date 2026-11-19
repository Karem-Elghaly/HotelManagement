
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
      
        builder.ToTable("Reviews", t =>
            t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.ManagerReply).HasMaxLength(1000);

   
        builder.HasIndex(r => r.ReservationId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

      
        builder.HasOne(r => r.Reservation)
            .WithMany()
            .HasForeignKey(r => r.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

       
        builder.HasIndex(r => new { r.IsHidden, r.Rating });

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}