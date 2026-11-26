using System;
using System.Collections.Generic;

namespace InsurancePortal.QuoteService.Application.Dtos
{
    /// <summary>
    /// Request DTO for creating a new quote
    /// Contains all required information to create a quote with product quotes
    /// </summary>
    public class CreateQuoteRequest
    {
        /// <summary>
        /// User ID (from identity system) - required
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User-facing reference number for quote - required, must be unique
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// FK to QuoteStatusLookup - typically "Draft" status
        /// </summary>
        public Guid StatusId { get; set; }

        /// <summary>
        /// When this quote expires - required
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// One or more product quotes (Auto, Home, Health)
        /// </summary>
        public List<CreateProductQuoteRequest> ProductQuotes { get; set; } = new();
    }

    /// <summary>
    /// Response DTO when quote is successfully created
    /// Includes lookup names for display
    /// </summary>
    public class QuoteResponse
    {
        /// <summary>
        /// Unique quote identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User-facing reference number
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Display name of the quote status (from lookup)
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Total premium across all products
        /// </summary>
        public decimal TotalPremium { get; set; }

        /// <summary>
        /// When this quote expires
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// When the quote was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// All product quotes for this request
        /// </summary>
        public List<ProductQuoteResponse> ProductQuotes { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for creating a product quote (offer)
    /// This is the offer portion of a quote - contains pricing and provider info
    /// </summary>
    public class CreateProductQuoteRequest
    {
        /// <summary>
        /// FK to ProductTypeLookup (Auto, Home, or Health) - required
        /// </summary>
        public Guid ProductTypeId { get; set; }

        /// <summary>
        /// FK to CurrencyLookup (USD or CAD) - required
        /// </summary>
        public Guid CurrencyId { get; set; }

        /// <summary>
        /// The premium being quoted for this product
        /// </summary>
        public decimal QuotedPremium { get; set; }

        /// <summary>
        /// FK to InsuranceProviderLookup - optional (can be null for system-generated quotes)
        /// </summary>
        public Guid? ProviderId { get; set; }

        /// <summary>
        /// Who provided this quote (agent name, system name, etc.) - optional
        /// </summary>
        public string QuotedBy { get; set; }

        /// <summary>
        /// FK to QuotedByTypeLookup (System, Agent, Associate) - optional
        /// </summary>
        public Guid? QuotedByTypeId { get; set; }

        /// <summary>
        /// Auto insurance specific details (when ProductTypeId is Auto)
        /// </summary>
        public AutoQuoteDetailsDto AutoDetails { get; set; }

        /// <summary>
        /// Home insurance specific details (when ProductTypeId is Home)
        /// </summary>
        public HomeQuoteDetailsDto HomeDetails { get; set; }

        /// <summary>
        /// Health insurance specific details (when ProductTypeId is Health)
        /// </summary>
        public HealthQuoteDetailsDto HealthDetails { get; set; }
    }

    /// <summary>
    /// Response DTO for a product quote (offer)
    /// Includes lookup names for display instead of GUIDs
    /// </summary>
    public class ProductQuoteResponse
    {
        /// <summary>
        /// Unique product quote identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product type name (Auto, Home, Health from lookup)
        /// </summary>
        public string ProductTypeName { get; set; }

        /// <summary>
        /// Currency code (USD or CAD from lookup)
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Offer status name (Pending, Offered, Accepted, etc. from lookup)
        /// </summary>
        public string OfferStatusName { get; set; }

        /// <summary>
        /// The quoted premium
        /// </summary>
        public decimal QuotedPremium { get; set; }

        /// <summary>
        /// When this offer expires
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// Insurance provider name (from lookup, may be null)
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Who quoted this (may be system or agent name)
        /// </summary>
        public string QuotedBy { get; set; }

        /// <summary>
        /// Type of quoter (System, Agent, Associate from lookup, may be null)
        /// </summary>
        public string QuotedByTypeName { get; set; }

        /// <summary>
        /// When the quote was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Auto insurance details (if ProductTypeName is "Auto")
        /// </summary>
        public AutoQuoteDetailsDto AutoDetails { get; set; }

        /// <summary>
        /// Home insurance details (if ProductTypeName is "Home")
        /// </summary>
        public HomeQuoteDetailsDto HomeDetails { get; set; }

        /// <summary>
        /// Health insurance details (if ProductTypeName is "Health")
        /// </summary>
        public HealthQuoteDetailsDto HealthDetails { get; set; }
    }

    /// <summary>
    /// Auto insurance underwriting details DTO
    /// These are the application inputs, not pricing
    /// </summary>
    public class AutoQuoteDetailsDto
    {
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string DrivingRecord { get; set; }
        public int NumberOfDrivers { get; set; }
        public decimal EstimatedAnnualMileage { get; set; }
    }

    /// <summary>
    /// Home insurance underwriting details DTO
    /// These are the application inputs, not pricing
    /// </summary>
    public class HomeQuoteDetailsDto
    {
        public string HomeAddress { get; set; }
        public int SquareFootage { get; set; }
        public int YearBuilt { get; set; }
        public string HomeType { get; set; }
        public decimal HomeValue { get; set; }
        public bool HasSecuritySystem { get; set; }
        public bool HasFireAlarm { get; set; }
        public int NumberOfOccupants { get; set; }
    }

    /// <summary>
    /// Health insurance underwriting details DTO
    /// These are the application inputs, not pricing
    /// </summary>
    public class HealthQuoteDetailsDto
    {
        public int Age { get; set; }
        public string HealthStatus { get; set; }
        public bool HasPreexistingConditions { get; set; }
        public bool Smoker { get; set; }
        public string CoverageType { get; set; }
        public int Dependents { get; set; }
    }

    /// <summary>
    /// DTO for location information
    /// Used when creating/updating home quotes with normalized addresses
    /// </summary>
    public class LocationDto
    {
        public Guid? Id { get; set; }
        public int LocationTypeId { get; set; }
        public string LocationTypeName { get; set; }
        public string UnitNumber { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public Guid ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public Guid CountryId { get; set; }
        public string CountryName { get; set; }
    }

    /// <summary>
    /// Generic lookup DTO for returning lookup values
    /// Used for GET /api/lookups endpoints
    /// </summary>
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Specialized currency lookup DTO
    /// Includes ISO code and decimal places for formatting
    /// </summary>
    public class CurrencyLookupDto : LookupDto
    {
        public string IsoCode { get; set; }
        public int DecimalPlaces { get; set; }
    }

    /// <summary>
    /// Specialized provider lookup DTO
    /// Includes contact information
    /// </summary>
    public class ProviderLookupDto : LookupDto
    {
        public string RegistrationNumber { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
    }
}
