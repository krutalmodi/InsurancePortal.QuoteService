using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    public partial class NullableLookupUpdateAudit : Migration
    {
        private static readonly (string Table, int LastSeedId)[] LookupTables =
        {
            ("QuoteStatusLookups", 8),
            ("ProductTypeLookups", 3),
            ("ProductQuoteStatusLookups", 6),
            ("QuotedByTypeLookups", 3),
            ("CurrencyLookups", 2),
            ("InsuranceProviderLookups", 1),
            ("VehicleColorLookups", 9),
            ("VehicleTypeLookups", 11),
            ("DrivingRecordTypeLookups", 7),
            ("HomeTypeLookups", 6),
            ("HealthStatusLookups", 7),
            ("CoverageTypeLookups", 3),
            ("SmokingStatusLookups", 3),
            ("VehicleMakeLookups", 9),
            ("VehicleModelLookups", 5)
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, lastSeedId) in LookupTables)
            {
                migrationBuilder.AlterColumn<DateTime>(
                    name: "UpdatedAt", table: table, type: "datetime2", nullable: true,
                    oldClrType: typeof(DateTime), oldType: "datetime2");

                migrationBuilder.Sql($"UPDATE [{table}] SET [UpdatedAt] = NULL, [UpdatedBy] = NULL WHERE [Id] BETWEEN 1 AND {lastSeedId} AND [CreatedBy] = 'seed' AND [CreatedAt] = '2024-01-01T00:00:00' AND [UpdatedBy] = 'seed' AND [UpdatedAt] = '2024-01-01T00:00:00'");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, _) in LookupTables)
            {
                migrationBuilder.Sql($"UPDATE [{table}] SET [UpdatedAt] = [CreatedAt] WHERE [UpdatedAt] IS NULL");
                migrationBuilder.AlterColumn<DateTime>(
                    name: "UpdatedAt", table: table, type: "datetime2", nullable: false,
                    oldClrType: typeof(DateTime), oldType: "datetime2", oldNullable: true);
            }
        }
    }
}
