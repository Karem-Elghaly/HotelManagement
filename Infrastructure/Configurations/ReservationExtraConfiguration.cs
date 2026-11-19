
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class ReservationExtraConfiguration : IEntityTypeConfiguration<ReservationExtra>
{
    public void Configure(EntityTypeBuilder<ReservationExtra> builder)
    {
        builder.ToTable("ReservationExtras", t =>
            t.HasCheckConstraint("CK_ReservationExtras_Quantity", "[Quantity] > 0"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);

        
        builder.Ignore(e => e.Total);

        builder.HasOne(e => e.Reservation)
            .WithMany()
            .HasForeignKey(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

       
        builder.HasOne<ExtraService>()
            .WithMany()
            .HasForeignKey(e => e.ExtraServiceId)
            .OnDelete(DeleteBehavior.Restrict);

   
        builder.HasIndex(e => e.ReservationId);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}