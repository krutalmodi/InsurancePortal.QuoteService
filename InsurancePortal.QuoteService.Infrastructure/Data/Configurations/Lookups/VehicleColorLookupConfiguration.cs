using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations.Lookups
{
    public class VehicleColorLookupConfiguration : IEntityTypeConfiguration<VehicleColorLookup>
    {
        public void Configure(EntityTypeBuilder<VehicleColorLookup> builder)
        {
            builder.ToTable("VehicleColorLookups");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2");

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("datetime2");
        }
    }
}
