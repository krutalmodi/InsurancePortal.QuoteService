using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypeLookups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "VehicleTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Sedan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Sedan", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "SUV", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "SUV", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "PickupTruck", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Pickup Truck", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Hatchback", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Hatchback", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "Coupe", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Coupe", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Convertible", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Convertible", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Wagon", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Wagon", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 8, "Minivan", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Minivan", 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 9, "Van", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Van", 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 10, "Motorcycle", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Motorcycle", 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 11, "Other", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Other", 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypeLookups_Code",
                table: "VehicleTypeLookups",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleTypeLookups");
        }
    }
}
