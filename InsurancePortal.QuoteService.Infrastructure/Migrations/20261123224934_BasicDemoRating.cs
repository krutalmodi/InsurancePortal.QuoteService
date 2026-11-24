using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BasicDemoRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RatingSnapshot",
                table: "ProductQuotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostalRisks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetVersion = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Hail = table.Column<int>(type: "int", nullable: false),
                    Theft = table.Column<int>(type: "int", nullable: false),
                    Fire = table.Column<int>(type: "int", nullable: false),
                    Crash = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalRisks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IsPercentageOfHomeValue = table.Column<bool>(type: "bit", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingFactors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatingPlanId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Minimum = table.Column<int>(type: "int", nullable: false),
                    Maximum = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingFactors_RatingPlans_RatingPlanId",
                        column: x => x.RatingPlanId,
                        principalTable: "RatingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostalRisks_DatasetVersion_PostalCode",
                table: "PostalRisks",
                columns: new[] { "DatasetVersion", "PostalCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RatingFactors_RatingPlanId_Type_Minimum",
                table: "RatingFactors",
                columns: new[] { "RatingPlanId", "Type", "Minimum" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalRisks");

            migrationBuilder.DropTable(
                name: "RatingFactors");

            migrationBuilder.DropTable(
                name: "RatingPlans");

            migrationBuilder.DropColumn(
                name: "RatingSnapshot",
                table: "ProductQuotes");
        }
    }
}
