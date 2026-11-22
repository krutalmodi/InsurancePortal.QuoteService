using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for HomeQuote (Table-Per-Type inheritance)
    /// Maps to separate HomeQuotes table with home-specific underwriting inputs
    /// </summary>
    public class HomeQuoteConfiguration : IEntityTypeConfiguration<HomeQuote>
    {
        public void Configure(EntityTypeBuilder<HomeQuote> builder)
        {
            // Map to separate HomeQuotes table
            builder.ToTable("HomeQuotes");

            // Location is owned by LocationService; address is stored as a snapshot on the quote
            builder.Property(hq => hq.LocationId);

            // Configure the Details value object as owned entity with column prefix flattened
            // These are underwriting inputs, not offer data (which stays in ProductQuote)
            builder.OwnsOne(hq => hq.Details, od =>
            {
                od.Property(d => d.SquareFootage)
                    .HasColumnName("SquareFootage")
                    .IsRequired();

                od.Property(d => d.YearBuilt)
                    .HasColumnName("YearBuilt")
                    .IsRequired();

                od.Property(d => d.HomeType)
                    .HasColumnName("HomeType")
                    .HasMaxLength(100);

                od.Property(d => d.HomeValue)
                    .HasColumnName("HomeValue")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                od.Property(d => d.HasSecuritySystem)
                    .HasColumnName("HasSecuritySystem")
                    .IsRequired();

                od.Property(d => d.HasFireAlarm)
                    .HasColumnName("HasFireAlarm")
                    .IsRequired();

                od.Property(d => d.NumberOfOccupants)
                    .HasColumnName("NumberOfOccupants")
                    .IsRequired();
            });

            }
    }
}