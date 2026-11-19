using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);

        
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);

        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.UserId).IsRequired();

      
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

   
        builder.Ignore(t => t.IsRevoked);
        builder.Ignore(t => t.IsExpired);
    }
}