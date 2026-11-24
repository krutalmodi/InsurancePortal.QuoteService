using InsurancePortal.QuoteService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Configurations
{
    public static class RatingConfiguration
    {
        public static void ConfigureRating(ModelBuilder modelBuilder)
        {
            var plans = modelBuilder.Entity<RatingPlan>();
            plans.ToTable("RatingPlans");
            plans.Property(p => p.Name).HasMaxLength(100).IsRequired();
            plans.Property(p => p.BaseRate).HasPrecision(18, 6);

            var factors = modelBuilder.Entity<RatingFactor>();
            factors.ToTable("RatingFactors");
            factors.Property(f => f.Type).HasMaxLength(40).IsRequired();
            factors.Property(f => f.Multiplier).HasPrecision(12, 6);
            factors.HasOne<RatingPlan>().WithMany().HasForeignKey(f => f.RatingPlanId).OnDelete(DeleteBehavior.Restrict);
            factors.HasIndex(f => new { f.RatingPlanId, f.Type, f.Minimum }).IsUnique();

            var risks = modelBuilder.Entity<PostalRisk>();
            risks.ToTable("PostalRisks");
            risks.Property(r => r.PostalCode).HasMaxLength(16).IsRequired();
            risks.HasIndex(r => new { r.CountryId, r.ProvinceId, r.DatasetVersion, r.PostalCode }).IsUnique();
        }
    }
}
