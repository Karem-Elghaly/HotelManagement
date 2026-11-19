
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class SeasonalRateConfiguration : IEntityTypeConfiguration<SeasonalRate>
{
    public void Configure(EntityTypeBuilder<SeasonalRate> builder)
    {
      
        builder.ToTable("SeasonalRates", t =>
        {
            t.HasCheckConstraint("CK_SeasonalRates_Multiplier", "[Multiplier] > 0");
            t.HasCheckConstraint("CK_SeasonalRates_Dates", "[EndDate] > [StartDate]");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Multiplier).HasPrecision(5, 2);

    
        builder.HasOne<RoomType>()
            .WithMany()
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.RoomTypeId, r.StartDate, r.EndDate });

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}