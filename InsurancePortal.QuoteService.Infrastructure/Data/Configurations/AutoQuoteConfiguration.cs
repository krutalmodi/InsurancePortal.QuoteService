using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for AutoQuote (Table-Per-Type inheritance)
    /// Maps to separate AutoQuotes table with vehicle-specific underwriting inputs
    /// Inherits offer data (premium, status, currency, provider) from ProductQuote base
    /// </summary>
    public class AutoQuoteConfiguration : IEntityTypeConfiguration<AutoQuote>
    {
        public void Configure(EntityTypeBuilder<AutoQuote> builder)
        {
            // Map to separate AutoQuotes table
            builder.ToTable("AutoQuotes");

            // Configure the Details value object as owned entity with flattened columns
            // These are underwriting inputs, not offer data (which stays in ProductQuote)
            builder.OwnsOne(aq => aq.Details, od =>
            {
                od.Property(d => d.VIN)
                    .HasColumnName("VIN")
                    .HasMaxLength(20)
                    .IsRequired();

                od.Property(d => d.VehicleMakeId)
                    .HasColumnName("VehicleMakeId")
                    .IsRequired();

                od.Property(d => d.VehicleModelId)
                    .HasColumnName("VehicleModelId")
                    .IsRequired();

                od.Property(d => d.Year)
                    .HasColumnName("Year")
                    .IsRequired();

                od.Property(d => d.VehicleColorId)
                    .HasColumnName("VehicleColorId")
                    .IsRequired();

                od.Property(d => d.Mileage)
                    .HasColumnName("Mileage")
                    .IsRequired();

                od.Property(d => d.DrivingRecord)
                    .HasColumnName("DrivingRecord")
                    .HasMaxLength(100);

                od.Property(d => d.NumberOfDrivers)
                    .HasColumnName("NumberOfDrivers")
                    .IsRequired();

                od.Property(d => d.EstimatedAnnualMileage)
                    .HasColumnName("EstimatedAnnualMileage")
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();

                od.HasOne<VehicleMakeLookup>()
                    .WithMany()
                    .HasForeignKey(d => d.VehicleMakeId)
                    .OnDelete(DeleteBehavior.Restrict);

                od.HasOne<VehicleModelLookup>()
                    .WithMany()
                    .HasForeignKey(d => d.VehicleModelId)
                    .OnDelete(DeleteBehavior.Restrict);

                od.HasOne<VehicleColorLookup>()
                    .WithMany()
                    .HasForeignKey(d => d.VehicleColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            }
    }
}