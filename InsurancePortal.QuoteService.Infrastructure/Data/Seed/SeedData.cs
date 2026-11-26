using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using System;
using System.Collections.Generic;

namespace InsurancePortal.QuoteService.Infrastructure.Data.Seed
{
    /// <summary>
    /// Deterministic seed data for lookups
    /// All lookup IDs are fixed integers and timestamps are hardcoded for repeatable migrations
    /// </summary>
    public static class SeedData
    {
        private static readonly DateTime SeedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Seed all lookup data into the model builder
        /// </summary>
        public static void SeedLookups(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuoteStatusLookup>().HasData(GetQuoteStatusLookups());
            modelBuilder.Entity<ProductTypeLookup>().HasData(GetProductTypeLookups());
            modelBuilder.Entity<ProductQuoteStatusLookup>().HasData(GetProductQuoteStatusLookups());
            modelBuilder.Entity<QuotedByTypeLookup>().HasData(GetQuotedByTypeLookups());
            modelBuilder.Entity<CurrencyLookup>().HasData(GetCurrencyLookups());
            modelBuilder.Entity<InsuranceProviderLookup>().HasData(GetInsuranceProviderLookups());
            modelBuilder.Entity<VehicleColorLookup>().HasData(GetVehicleColorLookups());
            modelBuilder.Entity<VehicleTypeLookup>().HasData(GetVehicleTypeLookups());
            modelBuilder.Entity<DrivingRecordTypeLookup>().HasData(GetDrivingRecordTypeLookups());
            modelBuilder.Entity<HomeTypeLookup>().HasData(GetHomeTypeLookups());
            modelBuilder.Entity<HealthStatusLookup>().HasData(GetHealthStatusLookups());
            modelBuilder.Entity<CoverageTypeLookup>().HasData(GetCoverageTypeLookups());
            modelBuilder.Entity<SmokingStatusLookup>().HasData(GetSmokingStatusLookups());
            modelBuilder.Entity<VehicleMakeLookup>().HasData(GetVehicleMakeLookups());
            modelBuilder.Entity<VehicleModelLookup>().HasData(GetVehicleModelLookups());
        }

        /// <summary>
        /// Quote Status: Draft, Submitted, InProgress, Quoted, Expired, Accepted, Converted, Declined
        /// </summary>
        private static QuoteStatusLookup[] GetQuoteStatusLookups()
        {
            return new[]
            {
                new QuoteStatusLookup { Id = 1, Code = "Draft", Name = "Draft", Description = "Quote is in draft", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 2, Code = "Submitted", Name = "Submitted", Description = "Quote submitted", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 3, Code = "InProgress", Name = "In Progress", Description = "Quote being processed", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 4, Code = "Quoted", Name = "Quoted", Description = "Quote complete", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 5, Code = "Expired", Name = "Expired", Description = "Quote has expired", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 6, Code = "Accepted", Name = "Accepted", Description = "Quote accepted", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 7, Code = "Converted", Name = "Converted", Description = "Converted to policy", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuoteStatusLookup { Id = 8, Code = "Declined", Name = "Declined", Description = "Quote declined", IsActive = true, SortOrder = 8, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Product Types: Auto, Home, Health
        /// </summary>
        private static ProductTypeLookup[] GetProductTypeLookups()
        {
            return new[]
            {
                new ProductTypeLookup { Id = 1, Code = "Auto", Name = "Auto Insurance", Description = "Automobile insurance", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductTypeLookup { Id = 2, Code = "Home", Name = "Home Insurance", Description = "Home/property insurance", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductTypeLookup { Id = 3, Code = "Health", Name = "Health Insurance", Description = "Health/medical insurance", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Product Quote Status: Pending, Offered, Accepted, Rejected, Expired, Withdrawn
        /// </summary>
        private static ProductQuoteStatusLookup[] GetProductQuoteStatusLookups()
        {
            return new[]
            {
                new ProductQuoteStatusLookup { Id = 1, Code = "Pending", Name = "Pending", Description = "Offer pending", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductQuoteStatusLookup { Id = 2, Code = "Offered", Name = "Offered", Description = "Offer provided", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductQuoteStatusLookup { Id = 3, Code = "Accepted", Name = "Accepted", Description = "Offer accepted", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductQuoteStatusLookup { Id = 4, Code = "Rejected", Name = "Rejected", Description = "Offer rejected", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductQuoteStatusLookup { Id = 5, Code = "Expired", Name = "Expired", Description = "Offer expired", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new ProductQuoteStatusLookup { Id = 6, Code = "Withdrawn", Name = "Withdrawn", Description = "Offer withdrawn", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Quoted By Type: System, Agent, Associate
        /// </summary>
        private static QuotedByTypeLookup[] GetQuotedByTypeLookups()
        {
            return new[]
            {
                new QuotedByTypeLookup { Id = 1, Code = "System", Name = "System", Description = "Automated system", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuotedByTypeLookup { Id = 2, Code = "Agent", Name = "Agent", Description = "Licensed agent", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new QuotedByTypeLookup { Id = 3, Code = "Associate", Name = "Associate", Description = "Insurance associate", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Currency: USD, CAD
        /// </summary>
        private static CurrencyLookup[] GetCurrencyLookups()
        {
            return new[]
            {
                new CurrencyLookup { Id = 1, Code = "USD", Name = "US Dollar", IsoCode = "USD", DecimalPlaces = 2, IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new CurrencyLookup { Id = 2, Code = "CAD", Name = "Canadian Dollar", IsoCode = "CAD", DecimalPlaces = 2, IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Insurance Provider: Allstate
        /// </summary>
        private static InsuranceProviderLookup[] GetInsuranceProviderLookups()
        {
            return new[]
            {
                new InsuranceProviderLookup { Id = 1, Code = "AllstateInsurance", Name = "Allstate Insurance", RegistrationNumber = "REG-00001", ContactEmail = "contact@allstate.com", PhoneNumber = "1-800-ALLSTATE", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Vehicle Colors
        /// </summary>
        private static VehicleColorLookup[] GetVehicleColorLookups()
        {
            return new[]
            {
                new VehicleColorLookup { Id = 1, Code = "Black", Name = "Black", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 2, Code = "White", Name = "White", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 3, Code = "Silver", Name = "Silver", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 4, Code = "Gray", Name = "Gray", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 5, Code = "Red", Name = "Red", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 6, Code = "Blue", Name = "Blue", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 7, Code = "Green", Name = "Green", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 8, Code = "Brown", Name = "Brown", IsActive = true, SortOrder = 8, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleColorLookup { Id = 9, Code = "Gold", Name = "Gold", IsActive = true, SortOrder = 9, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        private static VehicleTypeLookup[] GetVehicleTypeLookups()
        {
            return new[]
            {
                new VehicleTypeLookup { Id = 1, Code = "Sedan", Name = "Sedan", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 2, Code = "SUV", Name = "SUV", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 3, Code = "PickupTruck", Name = "Pickup Truck", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 4, Code = "Hatchback", Name = "Hatchback", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 5, Code = "Coupe", Name = "Coupe", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 6, Code = "Convertible", Name = "Convertible", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 7, Code = "Wagon", Name = "Wagon", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 8, Code = "Minivan", Name = "Minivan", IsActive = true, SortOrder = 8, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 9, Code = "Van", Name = "Van", IsActive = true, SortOrder = 9, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 10, Code = "Motorcycle", Name = "Motorcycle", IsActive = true, SortOrder = 10, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleTypeLookup { Id = 11, Code = "Other", Name = "Other", IsActive = true, SortOrder = 11, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Driving Record Types
        /// </summary>
        private static DrivingRecordTypeLookup[] GetDrivingRecordTypeLookups()
        {
            return new[]
            {
                new DrivingRecordTypeLookup { Id = 1, Code = "Clean", Name = "Clean", Description = "No violations", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 2, Code = "MinorViolation", Name = "Minor Violation", Description = "Minor traffic violation", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 3, Code = "MajorViolation", Name = "Major Violation", Description = "Major traffic violation", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 4, Code = "Accident", Name = "Accident", Description = "At-fault accident", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 5, Code = "DUI", Name = "DUI", Description = "Driving under influence", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 6, Code = "Suspension", Name = "Suspension", Description = "License suspended", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new DrivingRecordTypeLookup { Id = 7, Code = "Multiple", Name = "Multiple Violations", Description = "Multiple violations", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Home Types
        /// </summary>
        private static HomeTypeLookup[] GetHomeTypeLookups()
        {
            return new[]
            {
                new HomeTypeLookup { Id = 1, Code = "Detached", Name = "Detached", Description = "Detached home", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HomeTypeLookup { Id = 2, Code = "SemiDetached", Name = "Semi-Detached", Description = "Semi-detached home", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HomeTypeLookup { Id = 3, Code = "Townhouse", Name = "Townhouse", Description = "Townhouse", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HomeTypeLookup { Id = 4, Code = "Apartment", Name = "Apartment", Description = "Apartment/Condo", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HomeTypeLookup { Id = 5, Code = "MobileHome", Name = "Mobile Home", Description = "Mobile/manufactured home", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HomeTypeLookup { Id = 6, Code = "Other", Name = "Other", Description = "Other home type", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Health Status
        /// </summary>
        private static HealthStatusLookup[] GetHealthStatusLookups()
        {
            return new[]
            {
                new HealthStatusLookup { Id = 1, Code = "Excellent", Name = "Excellent", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 2, Code = "Good", Name = "Good", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 3, Code = "Fair", Name = "Fair", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 4, Code = "Poor", Name = "Poor", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 5, Code = "PreexistingCondition", Name = "Pre-existing Condition", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 6, Code = "Chronic", Name = "Chronic Disease", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new HealthStatusLookup { Id = 7, Code = "Disability", Name = "Disability", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Coverage Types
        /// </summary>
        private static CoverageTypeLookup[] GetCoverageTypeLookups()
        {
            return new[]
            {
                new CoverageTypeLookup { Id = 1, Code = "Basic", Name = "Basic", Description = "Basic coverage", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new CoverageTypeLookup { Id = 2, Code = "Standard", Name = "Standard", Description = "Standard coverage", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new CoverageTypeLookup { Id = 3, Code = "Premium", Name = "Premium", Description = "Premium coverage", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Smoking Status
        /// </summary>
        private static SmokingStatusLookup[] GetSmokingStatusLookups()
        {
            return new[]
            {
                new SmokingStatusLookup { Id = 1, Code = "NonSmoker", Name = "Non-Smoker", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new SmokingStatusLookup { Id = 2, Code = "FormerSmoker", Name = "Former Smoker", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new SmokingStatusLookup { Id = 3, Code = "CurrentSmoker", Name = "Current Smoker", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Vehicle Makes
        /// </summary>
        private static VehicleMakeLookup[] GetVehicleMakeLookups()
        {
            return new[]
            {
                new VehicleMakeLookup { Id = 1, Code = "Toyota", Name = "Toyota", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 2, Code = "Honda", Name = "Honda", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 3, Code = "Ford", Name = "Ford", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 4, Code = "Chevrolet", Name = "Chevrolet", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 5, Code = "BMW", Name = "BMW", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 6, Code = "Mercedes", Name = "Mercedes-Benz", IsActive = true, SortOrder = 6, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 7, Code = "Audi", Name = "Audi", IsActive = true, SortOrder = 7, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 8, Code = "Volkswagen", Name = "Volkswagen", IsActive = true, SortOrder = 8, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleMakeLookup { Id = 9, Code = "Mazda", Name = "Mazda", IsActive = true, SortOrder = 9, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }

        /// <summary>
        /// Vehicle Models (sample for Toyota, VehicleMakeId = 1)
        /// In production, this would have hundreds of entries
        /// </summary>
        private static VehicleModelLookup[] GetVehicleModelLookups()
        {
            const int toyotaId = 1;

            return new[]
            {
                new VehicleModelLookup { Id = 1, VehicleMakeId = toyotaId, Code = "Camry", Name = "Camry", IsActive = true, SortOrder = 1, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleModelLookup { Id = 2, VehicleMakeId = toyotaId, Code = "Corolla", Name = "Corolla", IsActive = true, SortOrder = 2, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleModelLookup { Id = 3, VehicleMakeId = toyotaId, Code = "Sienna", Name = "Sienna", IsActive = true, SortOrder = 3, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleModelLookup { Id = 4, VehicleMakeId = toyotaId, Code = "RAV4", Name = "RAV4", IsActive = true, SortOrder = 4, CreatedAt = SeedTimestamp, CreatedBy = "seed" },
                new VehicleModelLookup { Id = 5, VehicleMakeId = toyotaId, Code = "4Runner", Name = "4Runner", IsActive = true, SortOrder = 5, CreatedAt = SeedTimestamp, CreatedBy = "seed" }
            };
        }
    }
}
