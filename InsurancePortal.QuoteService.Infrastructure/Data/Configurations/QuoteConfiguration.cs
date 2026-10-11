using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for Quote entity
    /// </summary>
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.UserId)
                .IsRequired();

            builder.Property(q => q.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(q => q.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(q => q.ValidUntil)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(q => q.TotalPremium)
                .HasColumnType("decimal(18,2)");

            builder.Property(q => q.ReferenceNumber)
                .HasMaxLength(50);

            // One-to-many relationship with ProductQuotes
            builder.HasMany(q => q.ProductQuotes)
                .WithOne()
                .HasForeignKey(pq => pq.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for common queries
            builder.HasIndex(q => q.UserId);
            builder.HasIndex(q => q.Status);
            builder.HasIndex(q => q.CreatedAt);
        }
    }
}
