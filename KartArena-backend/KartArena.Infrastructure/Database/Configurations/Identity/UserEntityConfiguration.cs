using KartArena.Domain.Entities.Identity;

namespace KartArena.Infrastructure.Database.Configurations.Identity;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> b)
    {
        b.ToTable("Users");

        b.HasKey(x => x.Id);

        b.HasIndex(x => x.Email)
            .IsUnique();

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

       
        b.Property(x => x.isEnabled)
            .HasDefaultValue(true);

        
    }
}