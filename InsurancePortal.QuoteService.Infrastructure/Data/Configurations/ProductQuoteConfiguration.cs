using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for ProductQuote hierarchy (Table-Per-Hierarchy)
    /// Configures HomeQuote, AutoQuote, and HealthQuote
    /// </summary>
    public class ProductQuoteConfiguration : IEntityTypeConfiguration<ProductQuote>
    {
        public void Configure(EntityTypeBuilder<ProductQuote> builder)
        {
            builder.HasKey(pq => pq.Id);

            builder.Property(pq => pq.QuoteId)
                .IsRequired();

            builder.Property(pq => pq.QuotedPremium)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(pq => pq.ValidUntil)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(pq => pq.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            // Table-Per-Hierarchy (TPH) configuration
            builder.HasDiscriminator<string>("Discriminator")
                .HasValue<HomeQuote>(nameof(HomeQuote))
                .HasValue<AutoQuote>(nameof(AutoQuote))
                .HasValue<HealthQuote>(nameof(HealthQuote));

            // Index for common queries
            builder.HasIndex(pq => pq.QuoteId);
        }
    }
}

