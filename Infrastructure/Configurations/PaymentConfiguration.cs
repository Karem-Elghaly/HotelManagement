
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
     
        builder.ToTable("Payments", t =>
            t.HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Reference).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Reference).IsUnique();

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Type).HasConversion<int>();
        builder.Property(p => p.Method).HasConversion<int>();
        builder.Property(p => p.Notes).HasMaxLength(300);

        
        builder.Property(p => p.CreatedByUserId).HasMaxLength(450);

      
        builder.HasOne(p => p.Reservation)
            .WithMany()
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ReservationId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}