using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoQuoteVehicleLookupIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "Make",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AutoQuotes");

            migrationBuilder.AddColumn<int>(
                name: "VehicleColorId",
                table: "AutoQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleMakeId",
                table: "AutoQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleModelId",
                table: "AutoQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutoQuotes_VehicleColorId",
                table: "AutoQuotes",
                column: "VehicleColorId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoQuotes_VehicleMakeId",
                table: "AutoQuotes",
                column: "VehicleMakeId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoQuotes_VehicleModelId",
                table: "AutoQuotes",
                column: "VehicleModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoQuotes_VehicleColorLookups_VehicleColorId",
                table: "AutoQuotes",
                column: "VehicleColorId",
                principalTable: "VehicleColorLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AutoQuotes_VehicleMakeLookups_VehicleMakeId",
                table: "AutoQuotes",
                column: "VehicleMakeId",
                principalTable: "VehicleMakeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AutoQuotes_VehicleModelLookups_VehicleModelId",
                table: "AutoQuotes",
                column: "VehicleModelId",
                principalTable: "VehicleModelLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoQuotes_VehicleColorLookups_VehicleColorId",
                table: "AutoQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_AutoQuotes_VehicleMakeLookups_VehicleMakeId",
                table: "AutoQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_AutoQuotes_VehicleModelLookups_VehicleModelId",
                table: "AutoQuotes");

            migrationBuilder.DropIndex(
                name: "IX_AutoQuotes_VehicleColorId",
                table: "AutoQuotes");

            migrationBuilder.DropIndex(
                name: "IX_AutoQuotes_VehicleMakeId",
                table: "AutoQuotes");

            migrationBuilder.DropIndex(
                name: "IX_AutoQuotes_VehicleModelId",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "VehicleColorId",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "VehicleMakeId",
                table: "AutoQuotes");

            migrationBuilder.DropColumn(
                name: "VehicleModelId",
                table: "AutoQuotes");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "AutoQuotes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Make",
                table: "AutoQuotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AutoQuotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
