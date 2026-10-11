using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Infrastructure.Data.Configurations;

namespace InsurancePortal.QuoteService.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Quote Service
    /// Handles database operations for Quotes and ProductQuotes
    /// </summary>
    public class QuoteDbContext : DbContext
    {
        public QuoteDbContext(DbContextOptions<QuoteDbContext> options) : base(options)
        {
        }

        public DbSet<Quote> Quotes { get; set; }
        public DbSet<ProductQuote> ProductQuotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new QuoteConfiguration());
            modelBuilder.ApplyConfiguration(new ProductQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new HomeQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new AutoQuoteConfiguration());
            modelBuilder.ApplyConfiguration(new HealthQuoteConfiguration());
        }
    }
}

