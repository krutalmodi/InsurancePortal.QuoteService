using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for HealthQuote (Table-Per-Type inheritance)
    /// Maps to separate HealthQuotes table with health-specific underwriting inputs
    /// Inherits offer data (premium, status, currency, provider) from ProductQuote base
    /// </summary>
    public class HealthQuoteConfiguration : IEntityTypeConfiguration<HealthQuote>
    {
        public void Configure(EntityTypeBuilder<HealthQuote> builder)
        {
            // Map to separate HealthQuotes table
            builder.ToTable("HealthQuotes");

            // Configure the Details value object as owned entity with flattened columns
            // These are underwriting inputs, not offer data (which stays in ProductQuote)
            builder.OwnsOne(hq => hq.Details, od =>
            {
                od.Property(d => d.Age)
                    .HasColumnName("Age")
                    .IsRequired();

                od.Property(d => d.HealthStatus)
                    .HasColumnName("HealthStatus")
                    .HasMaxLength(100)
                    .IsRequired();

                od.Property(d => d.HasPreexistingConditions)
                    .HasColumnName("HasPreexistingConditions")
                    .IsRequired();

                od.Property(d => d.Smoker)
                    .HasColumnName("Smoker")
                    .IsRequired();

                od.Property(d => d.CoverageType)
                    .HasColumnName("CoverageType")
                    .HasMaxLength(100);

                od.Property(d => d.Dependents)
                    .HasColumnName("Dependents")
                    .IsRequired();
            });
            }
    }
}