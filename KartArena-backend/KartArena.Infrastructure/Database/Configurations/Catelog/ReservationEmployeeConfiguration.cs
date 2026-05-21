using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KartArena.Infrastructure.Persistence.Configurations.Catalog
{
    public sealed class ReservationEmployeeEntityConfiguration
        : IEntityTypeConfiguration<ReservationEmployeeEntity>
    {
        public void Configure(EntityTypeBuilder<ReservationEmployeeEntity> builder)
        {
            builder.ToTable("ReservationEmployees");

            builder.HasKey(x => x.Id);

         

            // === Employee ===
            builder
                .HasOne(x => x.Employee)
                .WithMany() // change to .WithMany(e => e.ReservationEmployees) if navigation exists
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            // === Reservation ===
            builder
                .HasOne(x => x.Reservation)
                .WithMany(r => r.Employees)
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Equipment Item (optional) ===
            builder
                .HasOne(x => x.EquipmentItem)
                .WithMany(e => e.ReservationEmployees)
                .HasForeignKey(x => x.EquipmentItemId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // === Indexes ===
            builder.HasIndex(x => x.ReservationId);
            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.EquipmentItemId);

            // Prevent exact duplicate assignment
            builder
                .HasIndex(x => new { x.ReservationId, x.EmployeeId, x.EquipmentItemId })
                .IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}