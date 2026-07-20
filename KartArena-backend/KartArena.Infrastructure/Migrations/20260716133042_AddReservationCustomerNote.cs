using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartArena.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationCustomerNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "Reservations");
        }
    }
}
