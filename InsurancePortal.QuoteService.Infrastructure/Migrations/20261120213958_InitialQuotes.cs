using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsurancePortal.QuoteService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoverageTypeLookups",
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
                    table.PrimaryKey("PK_CoverageTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
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
                    table.PrimaryKey("PK_CurrencyLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrivingRecordTypeLookups",
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
                    table.PrimaryKey("PK_DrivingRecordTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthStatusLookups",
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
                    table.PrimaryKey("PK_HealthStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeTypeLookups",
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
                    table.PrimaryKey("PK_HomeTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceProviderLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_InsuranceProviderLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductQuoteStatusLookups",
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
                    table.PrimaryKey("PK_ProductQuoteStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypeLookups",
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
                    table.PrimaryKey("PK_ProductTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuotedByTypeLookups",
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
                    table.PrimaryKey("PK_QuotedByTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuoteStatusLookups",
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
                    table.PrimaryKey("PK_QuoteStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmokingStatusLookups",
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
                    table.PrimaryKey("PK_SmokingStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleColorLookups",
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
                    table.PrimaryKey("PK_VehicleColorLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleMakeLookups",
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
                    table.PrimaryKey("PK_VehicleMakeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 256, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_QuoteStatusLookups_StatusId",
                        column: x => x.StatusId,
                        principalTable: "QuoteStatusLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModelLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    VehicleMakeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_VehicleModelLookups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleModelLookups_VehicleMakeLookups_VehicleMakeId",
                        column: x => x.VehicleMakeId,
                        principalTable: "VehicleMakeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    QuotedPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    QuotedBy = table.Column<Guid>(type: "uniqueidentifier", maxLength: 256, nullable: true),
                    QuotedByTypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_CurrencyLookups_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "CurrencyLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_InsuranceProviderLookups_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "InsuranceProviderLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_ProductTypeLookups_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_QuoteStatusLookups_StatusId",
                        column: x => x.StatusId,
                        principalTable: "QuoteStatusLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_QuotedByTypeLookups_QuotedByTypeId",
                        column: x => x.QuotedByTypeId,
                        principalTable: "QuotedByTypeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductQuotes_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutoQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    VIN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    DrivingRecord = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumberOfDrivers = table.Column<int>(type: "int", nullable: true),
                    EstimatedAnnualMileage = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoQuotes_ProductQuotes_Id",
                        column: x => x.Id,
                        principalTable: "ProductQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasPreexistingConditions = table.Column<bool>(type: "bit", nullable: true),
                    Smoker = table.Column<bool>(type: "bit", nullable: true),
                    CoverageType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Dependents = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthQuotes_ProductQuotes_Id",
                        column: x => x.Id,
                        principalTable: "ProductQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    SquareFootage = table.Column<int>(type: "int", nullable: true),
                    YearBuilt = table.Column<int>(type: "int", nullable: true),
                    HomeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HomeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HasSecuritySystem = table.Column<bool>(type: "bit", nullable: true),
                    HasFireAlarm = table.Column<bool>(type: "bit", nullable: true),
                    NumberOfOccupants = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeQuotes_ProductQuotes_Id",
                        column: x => x.Id,
                        principalTable: "ProductQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CoverageTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Basic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Basic coverage", true, "Basic", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Standard", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Standard coverage", true, "Standard", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Premium", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Premium coverage", true, "Premium", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "CurrencyLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DecimalPlaces", "Description", "IsActive", "IsoCode", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "USD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 2, null, true, "USD", "US Dollar", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 2, null, true, "CAD", "Canadian Dollar", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "DrivingRecordTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Clean", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "No violations", true, "Clean", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "MinorViolation", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Minor traffic violation", true, "Minor Violation", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "MajorViolation", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Major traffic violation", true, "Major Violation", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Accident", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "At-fault accident", true, "Accident", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "DUI", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Driving under influence", true, "DUI", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Suspension", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "License suspended", true, "Suspension", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Multiple", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Multiple violations", true, "Multiple Violations", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "HealthStatusLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Excellent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Excellent", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Good", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Good", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Fair", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Fair", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Poor", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Poor", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "PreexistingCondition", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Pre-existing Condition", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Chronic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Chronic Disease", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Disability", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Disability", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "HomeTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Detached", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Detached home", true, "Detached", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "SemiDetached", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Semi-detached home", true, "Semi-Detached", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Townhouse", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Townhouse", true, "Townhouse", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Apartment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Apartment/Condo", true, "Apartment", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "MobileHome", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Mobile/manufactured home", true, "Mobile Home", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Other", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Other home type", true, "Other", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "InsuranceProviderLookups",
                columns: new[] { "Id", "Code", "ContactEmail", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "PhoneNumber", "RegistrationNumber", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, "AllstateInsurance", "contact@allstate.com", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Allstate Insurance", "1-800-ALLSTATE", "REG-00001", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" });

            migrationBuilder.InsertData(
                table: "ProductQuoteStatusLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Pending", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer pending", true, "Pending", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Offered", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer provided", true, "Offered", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Accepted", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer accepted", true, "Accepted", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Rejected", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer rejected", true, "Rejected", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "Expired", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer expired", true, "Expired", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Withdrawn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Offer withdrawn", true, "Withdrawn", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "ProductTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Auto", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Automobile insurance", true, "Auto Insurance", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Home", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Home/property insurance", true, "Home Insurance", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Health", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Health/medical insurance", true, "Health Insurance", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "QuoteStatusLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Draft", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote is in draft", true, "Draft", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Submitted", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote submitted", true, "Submitted", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "InProgress", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote being processed", true, "In Progress", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Quoted", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote complete", true, "Quoted", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "Expired", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote has expired", true, "Expired", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Accepted", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote accepted", true, "Accepted", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Converted", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Converted to policy", true, "Converted", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 8, "Declined", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Quote declined", true, "Declined", 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "QuotedByTypeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Automated system", true, "System", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Agent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Licensed agent", true, "Agent", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Associate", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Insurance associate", true, "Associate", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "SmokingStatusLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "NonSmoker", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Non-Smoker", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "FormerSmoker", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Former Smoker", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "CurrentSmoker", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Current Smoker", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "VehicleColorLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Black", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Black", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "White", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "White", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Silver", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Silver", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Gray", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Gray", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "Red", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Red", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Blue", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Blue", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Green", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Green", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 8, "Brown", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Brown", 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 9, "Gold", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Gold", 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "VehicleMakeLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Toyota", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Toyota", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 2, "Honda", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Honda", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 3, "Ford", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Ford", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 4, "Chevrolet", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Chevrolet", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 5, "BMW", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "BMW", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 6, "Mercedes", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Mercedes-Benz", 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 7, "Audi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Audi", 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 8, "Volkswagen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Volkswagen", 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" },
                    { 9, "Mazda", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Mazda", 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed" }
                });

            migrationBuilder.InsertData(
                table: "VehicleModelLookups",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "SortOrder", "UpdatedAt", "UpdatedBy", "VehicleMakeId" },
                values: new object[,]
                {
                    { 1, "Camry", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Camry", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1 },
                    { 2, "Corolla", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Corolla", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1 },
                    { 3, "Sienna", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "Sienna", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1 },
                    { 4, "RAV4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "RAV4", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1 },
                    { 5, "4Runner", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, true, "4Runner", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoverageTypeLookups_Code",
                table: "CoverageTypeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyLookups_Code",
                table: "CurrencyLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyLookups_IsoCode",
                table: "CurrencyLookups",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrivingRecordTypeLookups_Code",
                table: "DrivingRecordTypeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthStatusLookups_Code",
                table: "HealthStatusLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeTypeLookups_Code",
                table: "HomeTypeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProviderLookups_Code",
                table: "InsuranceProviderLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_CurrencyId",
                table: "ProductQuotes",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_ProductTypeId",
                table: "ProductQuotes",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_ProviderId",
                table: "ProductQuotes",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_QuotedByTypeId",
                table: "ProductQuotes",
                column: "QuotedByTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_QuoteId",
                table: "ProductQuotes",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_QuoteId_ProductTypeId",
                table: "ProductQuotes",
                columns: new[] { "QuoteId", "ProductTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_StatusId",
                table: "ProductQuotes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotes_ValidUntil",
                table: "ProductQuotes",
                column: "ValidUntil");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuoteStatusLookups_Code",
                table: "ProductQuoteStatusLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeLookups_Code",
                table: "ProductTypeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotedByTypeLookups_Code",
                table: "QuotedByTypeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreatedAt",
                table: "Quotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ReferenceNumber",
                table: "Quotes",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_StatusId",
                table: "Quotes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_UserId",
                table: "Quotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_UserId_CreatedAt",
                table: "Quotes",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteStatusLookups_Code",
                table: "QuoteStatusLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmokingStatusLookups_Code",
                table: "SmokingStatusLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleColorLookups_Code",
                table: "VehicleColorLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMakeLookups_Code",
                table: "VehicleMakeLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelLookups_Code",
                table: "VehicleModelLookups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelLookups_VehicleMakeId_Code",
                table: "VehicleModelLookups",
                columns: new[] { "VehicleMakeId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoQuotes");

            migrationBuilder.DropTable(
                name: "CoverageTypeLookups");

            migrationBuilder.DropTable(
                name: "DrivingRecordTypeLookups");

            migrationBuilder.DropTable(
                name: "HealthQuotes");

            migrationBuilder.DropTable(
                name: "HealthStatusLookups");

            migrationBuilder.DropTable(
                name: "HomeQuotes");

            migrationBuilder.DropTable(
                name: "HomeTypeLookups");

            migrationBuilder.DropTable(
                name: "ProductQuoteStatusLookups");

            migrationBuilder.DropTable(
                name: "SmokingStatusLookups");

            migrationBuilder.DropTable(
                name: "VehicleColorLookups");

            migrationBuilder.DropTable(
                name: "VehicleModelLookups");

            migrationBuilder.DropTable(
                name: "ProductQuotes");

            migrationBuilder.DropTable(
                name: "VehicleMakeLookups");

            migrationBuilder.DropTable(
                name: "CurrencyLookups");

            migrationBuilder.DropTable(
                name: "InsuranceProviderLookups");

            migrationBuilder.DropTable(
                name: "ProductTypeLookups");

            migrationBuilder.DropTable(
                name: "QuotedByTypeLookups");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "QuoteStatusLookups");
        }
    }
}
