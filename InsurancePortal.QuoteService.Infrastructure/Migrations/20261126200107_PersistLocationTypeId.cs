using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistLocationTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationTypeId",
                table: "HomeQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationTypeId",
                table: "HealthQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationTypeId",
                table: "AutoQuotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationTypeId",
                table: "HomeQuotes");

            migrationBuilder.DropColumn(
                name: "LocationTypeId",
                table: "HealthQuotes");

            migrationBuilder.DropColumn(
                name: "LocationTypeId",
                table: "AutoQuotes");
        }
    }
}
