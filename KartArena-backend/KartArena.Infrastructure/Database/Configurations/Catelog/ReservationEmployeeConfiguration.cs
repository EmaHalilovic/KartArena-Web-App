using KartArena.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KartArena.Infrastructure.Persistence.Configurations.Catalog
{
    public sealed class ReservationEmployeeEntityConfiguration
        : IEntityTypeConfiguration<ReservationEmployeeEntity>
    {
        public void Configure(EntityTypeBuilder<ReservationEmployeeEntity> builder)
        {
            builder.ToTable("ReservationEmployeeEntity");

            builder.HasKey(x => x.Id);

            // === Employee (User) ===
            builder
                .HasOne(x => x.Employee)
                .WithMany() // ako imaš kolekciju u UserEntity, promijeni u .WithMany(u => u.ReservationEmployees)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
            // ✅ ključno: nema cascade prema Users -> rješava multiple cascade paths

            // === Reservation ===
            builder
                .HasOne(x => x.Reservation)
                .WithMany(r => r.Employees) // PROMIJENI ako se kolekcija drugačije zove
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
            // ✅ logično: kad obrišeš rezervaciju, obriši i veze

            // === Equipment ===
            builder
                .HasOne(x => x.Equipment)
                .WithMany() // ako imaš kolekciju u EquipmentEntity, promijeni u .WithMany(e => e.ReservationEmployees)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.NoAction);
            // ✅ preporučeno: ne briši veze kaskadno kad obrišeš opremu (ili koristi Restrict)

            // === Indexes ===
            builder.HasIndex(x => x.ReservationId);
            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.EquipmentId);

            // Optional: spriječi duplikate (isti employee + equipment na istoj rezervaciji)
            builder
                .HasIndex(x => new { x.ReservationId, x.EmployeeId, x.EquipmentId })
                .IsUnique();
        }
    }
}
