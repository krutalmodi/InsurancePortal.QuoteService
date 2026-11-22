using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for Quote aggregate root
    /// Maps to Quotes table with audit fields and lookup foreign keys
    /// </summary>
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
        {
            builder.ToTable("Quotes");
            builder.HasKey(q => q.Id);

            // Core properties
            builder.Property(q => q.UserId)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(q => q.ReferenceNumber)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(q => q.ReferenceNumber).IsUnique();

            // Status - Foreign Key to QuoteStatusLookup
            builder.Property(q => q.StatusId)
                .IsRequired();

            // Timestamps
            builder.Property(q => q.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(q => q.ValidUntil)
                .IsRequired()
                .HasColumnType("datetime2");

            // Audit fields
            builder.Property(q => q.CreatedBy)
                .HasMaxLength(256);

            builder.Property(q => q.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(q => q.UpdatedBy)
                .HasMaxLength(256);

            // Premium
            builder.Property(q => q.TotalPremium)
                .HasColumnType("decimal(18,2)");

            // Obsolete Status enum - no longer mapped, preserved for backward compatibility
            // builder.Ignore(q => q.Status);

            // Foreign Keys
            builder.HasOne<QuoteStatusLookup>()
                .WithMany()
                .HasForeignKey(q => q.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many relationship with ProductQuotes (no cascade delete to preserve historical offers)
            builder.HasMany(q => q.ProductQuotes)
                .WithOne()
                .HasForeignKey(pq => pq.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common queries
            builder.HasIndex(q => q.UserId)
                .HasName("IX_Quotes_UserId");

            builder.HasIndex(q => q.StatusId)
                .HasName("IX_Quotes_StatusId");

            builder.HasIndex(q => new { q.UserId, q.CreatedAt })
                .HasName("IX_Quotes_UserId_CreatedAt");

            builder.HasIndex(q => q.CreatedAt)
                .HasName("IX_Quotes_CreatedAt");
        }
    }
}
