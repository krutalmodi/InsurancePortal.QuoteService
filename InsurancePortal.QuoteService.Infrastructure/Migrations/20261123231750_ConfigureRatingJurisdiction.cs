using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRatingJurisdiction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostalRisks_DatasetVersion_PostalCode",
                table: "PostalRisks");

            // Retain existing data for review, but deactivate the original demonstration plans.
            migrationBuilder.Sql("UPDATE RatingPlans SET Published = 0 WHERE (Id = 1 AND Name = 'Calgary NE Auto demo v1') OR (Id = 2 AND Name = 'Calgary NE Home demo v1')");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "RatingPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "RatingPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "RatingPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskDatasetVersion",
                table: "RatingPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "PostalRisks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "PostalRisks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostalRisks_CountryId_ProvinceId_DatasetVersion_PostalCode",
                table: "PostalRisks",
                columns: new[] { "CountryId", "ProvinceId", "DatasetVersion", "PostalCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostalRisks_CountryId_ProvinceId_DatasetVersion_PostalCode",
                table: "PostalRisks");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "RatingPlans");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "RatingPlans");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "RatingPlans");

            migrationBuilder.DropColumn(
                name: "RiskDatasetVersion",
                table: "RatingPlans");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "PostalRisks");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "PostalRisks");

            migrationBuilder.CreateIndex(
                name: "IX_PostalRisks_DatasetVersion_PostalCode",
                table: "PostalRisks",
                columns: new[] { "DatasetVersion", "PostalCode" },
                unique: true);
        }
    }
}
