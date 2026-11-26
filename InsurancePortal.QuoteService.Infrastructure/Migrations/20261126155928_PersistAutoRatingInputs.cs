using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistAutoRatingInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtFaultAccidentCount",
                table: "AutoQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpeedingTicketCount",
                table: "AutoQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeId",
                table: "AutoQuotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtFaultAccidentCount",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "SpeedingTicketCount",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "AutoQuotes");
        }
    }
}
