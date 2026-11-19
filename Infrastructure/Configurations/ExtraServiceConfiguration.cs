
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class ExtraServiceConfiguration : IEntityTypeConfiguration<ExtraService>
{
    public void Configure(EntityTypeBuilder<ExtraService> builder)
    {
        builder.ToTable("ExtraServices", t =>
            t.HasCheckConstraint("CK_ExtraServices_Price", "[Price] > 0"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Price).HasPrecision(18, 2);


        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(e => !e.IsDeleted);

      
        var seededAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new ExtraService { Id = 1, Name = "Breakfast", Description = "Buffet breakfast per person", Price = 15m, IsActive = true, CreatedAt = seededAt },
            new ExtraService { Id = 2, Name = "Airport Transfer", Description = "One-way private transfer", Price = 40m, IsActive = true, CreatedAt = seededAt },
            new ExtraService { Id = 3, Name = "Spa Session", Description = "60-minute massage", Price = 60m, IsActive = true, CreatedAt = seededAt });
    }
}