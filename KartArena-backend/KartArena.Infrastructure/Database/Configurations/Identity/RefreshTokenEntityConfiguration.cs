using KartArena.Domain.Entities.Identity;

namespace KartArena.Infrastructure.Database.Configurations.Identity;

public sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> b)
    {
        b.ToTable("RefreshTokens");

        b.HasKey(x => x.Id);

        // ✅ index on scalar FK, not navigation
        b.HasIndex(x => new { x.UserId, x.TokenHash })
            .IsUnique();

        b.Property(x => x.TokenHash)
            .IsRequired();

        b.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        b.Property(x => x.IsRevoked)
            .HasDefaultValue(false);

        b.Property(x => x.Fingerprint)
            .HasMaxLength(200);

        // ✅ navigation mapping
        b.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
