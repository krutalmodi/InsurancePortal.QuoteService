using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for AutoQuote
    /// </summary>
    public class AutoQuoteConfiguration : IEntityTypeConfiguration<AutoQuote>
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public void Configure(EntityTypeBuilder<AutoQuote> builder)
        {
            // Store Details as JSON in a single column
            builder.Property(aq => aq.Details)
                .HasColumnName("DetailsJson")
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Domain.ValueObjects.AutoQuoteDetails>(v, JsonOptions)
                );
        }
    }
}
