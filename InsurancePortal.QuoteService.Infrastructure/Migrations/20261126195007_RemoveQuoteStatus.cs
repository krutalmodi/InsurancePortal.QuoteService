using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuoteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Quotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
