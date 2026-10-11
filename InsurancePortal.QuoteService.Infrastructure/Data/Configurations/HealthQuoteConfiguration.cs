using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for HealthQuote
    /// </summary>
    public class HealthQuoteConfiguration : IEntityTypeConfiguration<HealthQuote>
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public void Configure(EntityTypeBuilder<HealthQuote> builder)
        {
            // Store Details as JSON in a single column
            builder.Property(hq => hq.Details)
                .HasColumnName("DetailsJson")
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Domain.ValueObjects.HealthQuoteDetails>(v, JsonOptions)
                );
        }
    }
}
