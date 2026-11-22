using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for ProductQuote base entity (Table-Per-Type)
    /// Configures offer metadata: premium, validity, status, currency, provider, agent
    /// HomeQuote, AutoQuote, and HealthQuote inherit from this and map to separate tables
    /// </summary>
    public class ProductQuoteConfiguration : IEntityTypeConfiguration<ProductQuote>
    {
        public void Configure(EntityTypeBuilder<ProductQuote> builder)
        {
            builder.HasKey(pq => pq.Id);

            // Core FK to Quote
            builder.Property(pq => pq.QuoteId)
                .IsRequired();

            // Offer metadata
            builder.Property(pq => pq.QuotedPremium)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(pq => pq.ValidUntil)
                .HasColumnType("datetime2")
                .IsRequired();

            // Lookup foreign keys
            builder.Property(pq => pq.StatusId)
                .IsRequired();

            builder.Property(pq => pq.ProductTypeId)
                .IsRequired();

            builder.Property(pq => pq.CurrencyId)
                .IsRequired();

            builder.Property(pq => pq.ProviderId);

            builder.Property(pq => pq.QuotedByTypeId);

            // Agent/System info
            builder.Property(pq => pq.QuotedBy)
                .HasMaxLength(256);

            // Audit fields
            builder.Property(pq => pq.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pq => pq.CreatedBy)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(pq => pq.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pq => pq.UpdatedBy)
                .HasMaxLength(256);

            // Foreign Keys to Lookups
            builder.HasOne<QuoteStatusLookup>()
                .WithMany()
                .HasForeignKey(pq => pq.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ProductTypeLookup>()
                .WithMany()
                .HasForeignKey(pq => pq.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CurrencyLookup>()
                .WithMany()
                .HasForeignKey(pq => pq.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InsuranceProviderLookup>()
                .WithMany()
                .HasForeignKey(pq => pq.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<QuotedByTypeLookup>()
                .WithMany()
                .HasForeignKey(pq => pq.QuotedByTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Foreign Key to Quote (no cascade delete to preserve historical offers)
            builder.HasOne<Quote>()
                .WithMany(q => q.ProductQuotes)
                .HasForeignKey(pq => pq.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common queries
            builder.HasIndex(pq => pq.QuoteId)
                .HasName("IX_ProductQuotes_QuoteId");

            builder.HasIndex(pq => pq.StatusId)
                .HasName("IX_ProductQuotes_StatusId");

            builder.HasIndex(pq => pq.ProductTypeId)
                .HasName("IX_ProductQuotes_ProductTypeId");

            builder.HasIndex(pq => pq.CurrencyId)
                .HasName("IX_ProductQuotes_CurrencyId");

            builder.HasIndex(pq => pq.ProviderId)
                .HasName("IX_ProductQuotes_ProviderId");

            builder.HasIndex(pq => pq.ValidUntil)
                .HasName("IX_ProductQuotes_ValidUntil");

            builder.HasIndex(pq => new { pq.QuoteId, pq.ProductTypeId })
                .HasName("IX_ProductQuotes_QuoteId_ProductTypeId");
        }
    }
}
