using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data.Configurations;
using InsurancePortal.QuoteService.Infrastructure.Data.Configurations.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data.Seed;

namespace InsurancePortal.QuoteService.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Quote Service
    /// Handles database operations for Quotes, ProductQuotes, and Lookup tables
    /// </summary>
    public class QuoteDbContext : DbContext
    {
        public QuoteDbContext(DbContextOptions<QuoteDbContext> options) : base(options)
        {
        }

        // Core Quote Entities
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<ProductQuote> ProductQuotes { get; set; }
        public DbSet<RatingPlan> RatingPlans { get; set; }
        public DbSet<RatingFactor> RatingFactors { get; set; }
        public DbSet<PostalRisk> PostalRisks { get; set; }

        // Separate DbSets for each product type (Table-Per-Type inheritance)
        public DbSet<HomeQuote> HomeQuotes { get; set; }
        public DbSet<AutoQuote> AutoQuotes { get; set; }
        public DbSet<HealthQuote> HealthQuotes { get; set; }

        // Lookup Entities - Simple Lookups (Tier 1)
        public DbSet<QuoteStatusLookup> QuoteStatusLookups { get; set; }
        public DbSet<ProductTypeLookup> ProductTypeLookups { get; set; }
        public DbSet<ProductQuoteStatusLookup> ProductQuoteStatusLookups { get; set; }
        public DbSet<QuotedByTypeLookup> QuotedByTypeLookups { get; set; }
        public DbSet<VehicleColorLookup> VehicleColorLookups { get; set; }
        public DbSet<VehicleTypeLookup> VehicleTypeLookups { get; set; }
        public DbSet<DrivingRecordTypeLookup> DrivingRecordTypeLookups { get; set; }
        public DbSet<HomeTypeLookup> HomeTypeLookups { get; set; }
        public DbSet<HealthStatusLookup> HealthStatusLookups { get; set; }
        public DbSet<CoverageTypeLookup> CoverageTypeLookups { get; set; }
        public DbSet<SmokingStatusLookup> SmokingStatusLookups { get; set; }

        // Lookup Entities - Special Lookups (Tier 2)
        public DbSet<CurrencyLookup> CurrencyLookups { get; set; }
        public DbSet<InsuranceProviderLookup> InsuranceProviderLookups { get; set; }

        // Lookup Entities - Hierarchical Lookups (Tier 3)
        public DbSet<VehicleMakeLookup> VehicleMakeLookups { get; set; }
        public DbSet<VehicleModelLookup> VehicleModelLookups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply core entity configurations
            modelBuilder.ApplyConfiguration(new QuoteConfiguration());
            modelBuilder.ApplyConfiguration(new ProductQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new HomeQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new AutoQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new HealthQuoteConfiguration());
            RatingConfiguration.ConfigureRating(modelBuilder);

            // Apply Tier 1 lookup configurations (simple lookups)
            modelBuilder.ApplyConfiguration(new QuoteStatusLookupConfiguration());
            modelBuilder.ApplyConfiguration(new ProductTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new ProductQuoteStatusLookupConfiguration());
            modelBuilder.ApplyConfiguration(new QuotedByTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleColorLookupConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new DrivingRecordTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new HomeTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new HealthStatusLookupConfiguration());
            modelBuilder.ApplyConfiguration(new CoverageTypeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new SmokingStatusLookupConfiguration());

            // Apply Tier 2 lookup configurations (special lookups)
            modelBuilder.ApplyConfiguration(new CurrencyLookupConfiguration());
            modelBuilder.ApplyConfiguration(new InsuranceProviderLookupConfiguration());

            // Apply Tier 3 lookup configurations (hierarchical lookups)
            modelBuilder.ApplyConfiguration(new VehicleMakeLookupConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleModelLookupConfiguration());

            // Lookup keys are int and seeded explicitly, so they must not be database-generated
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseLookup).IsAssignableFrom(t.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseLookup.Id)).ValueGeneratedNever();
            }

            // Seed lookup data (deterministic for migrations)
            modelBuilder.SeedLookups();
        }
    }
}