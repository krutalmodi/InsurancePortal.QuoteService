# Insurance Portal - Quote Service Architecture & Design

## Project Overview

**Quote Service** generates Home, Auto and Health quotes. The domain aggregate supports multiple product types, but each current API creation request creates a new quote with exactly one product. This document describes the current implementation; planned functionality is identified explicitly.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain Model Design](#domain-model-design)
3. [Database Design](#database-design)
4. [API Endpoints](#api-endpoints)
5. [CQRS Architecture](#cqrs-architecture)
6. [Design Patterns](#design-patterns)
7. [Project Structure](#project-structure)
8. [Service Layer Design](#service-layer-design)
9. [Implementation Notes](#implementation-notes)
10. [Table-Driven Rating](#table-driven-rating)
11. [Future Enhancements](#future-enhancements)
12. [Configuration](#configuration)
13. [Testing Strategy](#testing-strategy)
14. [Deployment](#deployment)
15. [Related Services](#related-services)
16. [References](#references)

---

## Architecture Overview

### High-Level Architecture

```text
Client -> QuotesController -> CQRS dispatcher -> command/query handler
												 |-> QuoteRepository -> SQL Server
												 |-> BasicRatingEngine -> SQL Server
												 `-> LocationClient -> LocationService
```

### Microservices Integration

- **Identity Service**: issues JWTs validated by the API using configured issuer, audience and signing key.
- **LocationService**: creates an address for each product quote through an HTTP client; the returned address is also snapshotted on the product quote. Location creation and quote persistence are separate operations, not a distributed transaction.
- **Policy, Billing and Notification services**: prospective consumers; current quote handlers do not publish events. A Service Bus publisher exists but is not registered or invoked.

---

## Domain Model Design

> The [table-driven rating engine](#table-driven-rating) is described below. The calculations in this section describe the legacy path when table-driven rating is disabled.

### Design Pattern: Table-Per-Type (TPT) with Composition

The `ProductQuotes` base table and separate product tables use TPT inheritance. Product details are EF-owned value objects flattened into each derived table.

### Key Domain Entities

#### 1. **Quote (Aggregate Root)**
Represents a complete quote request that can contain multiple product quotes.

**Properties:**
- `Id`: Unique identifier (GUID)
- `UserId`: External user identifier (GUID), not a database foreign key
- `StatusId`: Integer reference to `QuoteStatusLookups`; the obsolete `Status` property is also mapped
- `CreatedAt`: Timestamp of quote creation
- `ValidUntil`: Expiration date, set to 30 days from creation (`DateTime.UtcNow.AddDays(30)`)
- `TotalPremium`: Sum of all product premiums
- `ReferenceNumber`: Unique human-readable reference (QT-YYYYMMDD-XXXXX)
- `ProductQuotes`: Collection of product-specific quotes

**Key Methods:**
```csharp
void AddProductQuote(ProductQuote productQuote, decimal? calculatedPremium = null)
void RemoveProductQuote(ProductType productType)
void Validate()
void MarkAsConverted()                    // Set converted status ID; no policy is created
void MarkAsExpired()
bool IsExpired { get; }
```

**Business Rules:**
- A quote must contain at least one product quote
- Only one quote per product type per quote bundle
- All quotes are valid for 30 days from creation only; after that they are treated as expired and cannot be accepted
- Quote is automatically invalid if expired
- Premium is automatically calculated and summed

#### 2. **ProductQuote (Abstract Base Class)**
Abstract base class for all product-specific quotes using inheritance.

**Properties:**
- `Id`: GUID
- `QuoteId`: Foreign key to Quote
- `ProductType`: Abstract property (Home, Auto, Health)
- `QuotedPremium`: Calculated premium for this product
- `ValidUntil`: Inherited from parent quote
- `CreatedAt`: Timestamp
- `Discriminator`: Stored property; inheritance uses TPT, not a TPH discriminator mapping

**Abstract Methods:**
```csharp
abstract ProductType ProductType { get; }
abstract void Validate()
abstract decimal CalculatePremium()
```

#### 3. **HomeQuote** (Inherits from ProductQuote)
Home insurance quote with property-specific details.

**Product-Specific Properties:**
- `Details`: HomeQuoteDetails (value object with all home info)

**Premium Calculation Logic:**
- Base: 0.5% of home value
- Security system discount: -5%
- Fire alarm discount: -3%
- Age adjustment (>50 years): +10%

**Example:**
```csharp
HomeQuote home = new HomeQuote(
	new HomeQuoteDetails(
		squareFootage: 2500,
		yearBuilt: 1995,
		homeType: "Single Family",
		homeValue: 450000,
		hasSecuritySystem: true,
		hasFireAlarm: true,
		numberOfOccupants: 4
	)
);
```

#### 4. **AutoQuote** (Inherits from ProductQuote)
Auto insurance quote with vehicle-specific details.

**Product-Specific Properties:**
- `Details`: AutoQuoteDetails (value object with all vehicle info)

**Premium Calculation Logic:**
- Base: $1000
- Vehicle age (<3 years): +10%
- Vehicle age (>10 years): -5%
- High mileage (>100K): +5%
- Multiple drivers: +$150 per additional driver
- High annual mileage (>20K): +8%

#### 5. **HealthQuote** (Inherits from ProductQuote)
Health insurance quote with individual-specific details.

**Product-Specific Properties:**
- `Details`: HealthQuoteDetails (value object with all health info)

**Premium Calculation Logic:**
- Base: $500
- Age (<30): -20%
- Age (40 through 49): +25%
- Age (50 or older): +50%
- Pre-existing conditions: +35%
- Smoker: +50%
- Dependents: +$100 per dependent

### Value Objects

Value objects are immutable, validated at construction, and compared by value rather than reference.

#### HomeQuoteDetails
```csharp
public class HomeQuoteDetails
{
	// All immutable properties
	public int SquareFootage { get; }
	public int YearBuilt { get; }
	public string HomeType { get; }
	public decimal HomeValue { get; }
	public bool HasSecuritySystem { get; }
	public bool HasFireAlarm { get; }
	public int NumberOfOccupants { get; }

	// Validated in constructor
	// Implements Equals and GetHashCode
}
```

#### AutoQuoteDetails
```csharp
public class AutoQuoteDetails
{
	public string VIN { get; }
	public int VehicleMakeId { get; }   // FK -> VehicleMakeLookups
	public int VehicleModelId { get; }  // FK -> VehicleModelLookups
	public int Year { get; }
	public int VehicleColorId { get; }  // FK -> VehicleColorLookups
	public int Mileage { get; }
	public string DrivingRecord { get; }
	public int NumberOfDrivers { get; }
	public decimal EstimatedAnnualMileage { get; }
}
```

#### HealthQuoteDetails
```csharp
public class HealthQuoteDetails
{
	public int Age { get; }
	public string HealthStatus { get; }
	public bool HasPreexistingConditions { get; }
	public bool Smoker { get; }
	public string CoverageType { get; }
	public int Dependents { get; }
}
```

### Enumerations

**Quote status enums**

`QuoteStatus` has `Active = 1`, `Expired = 2`, `Converted = 3`, and `Rejected = 4`. The separate `QuoteStatusEnum` defines `Draft = 1`, `Submitted = 2`, `InProgress = 3`, `Quoted = 4`, `Accepted = 5`, `Rejected = 6`, `Expired = 7`, and `Cancelled = 8`. Creation uses the latter; `MarkAsConverted` and `MarkAsExpired` use the former. Their numeric meanings conflict, and the response maps the obsolete `Status` property rather than `StatusId`. Conversion does not create a policy.

**ProductType**
- `Auto`: Auto insurance (value = 1)
- `Home`: Home insurance (value = 2)
- `Health`: Health insurance (value = 3)

---

## Database Design

### Technology Stack
- **Database**: SQL Server
- **ORM**: Entity Framework Core 10.0
- **Inheritance**: TPT, with owned product detail objects flattened to product tables.

### Database Schema

The EF configurations and migrations in `InsurancePortal.QuoteService.Infrastructure\Data\Configurations` and `InsurancePortal.QuoteService.Infrastructure\Migrations` define the schema; the actual database depends on which migrations have been applied.

| Table | Key details |
|-------|-------------|
| `Quotes` | GUID ID and external `UserId`; integer `StatusId`; unique required `ReferenceNumber`; validity, audit fields and `TotalPremium` (`decimal(18,2)`). The obsolete `Status` enum is also mapped. |
| `ProductQuotes` | GUID ID and `QuoteId` FK; product, status, currency, provider and quoted-by lookup IDs; premium, validity and optional JSON `RatingSnapshot`. Deleting a parent quote with product rows is restricted. |
| `HomeQuotes`, `AutoQuotes`, `HealthQuotes` | Derived TPT tables share the base ID and store their respective owned details and address snapshots. `LocationId` is an external LocationService ID, not a local database FK. Auto make, model and color use integer lookup IDs. |
| Lookup tables | Integer IDs, explicitly seeded; vehicle models reference makes. See `QuoteDbContext` and its configurations for the current set and relationships. |
| `RatingPlans`, `RatingFactors`, `PostalRisks` | Configuration for the [rating engine](#table-driven-rating); the postal code column is `nvarchar(16)`. |

`(QuoteId, ProductTypeId)` has a non-unique index; the aggregate enforces one product per type, not a database unique constraint. `ProductQuotes.StatusId` is currently configured as a foreign key to `QuoteStatusLookups`, although product status values and comments refer to `ProductQuoteStatusLookups`.

```text
Quotes (1) ---< ProductQuotes
				   |-- HomeQuotes (TPT)
				   |-- AutoQuotes (TPT)
				   `-- HealthQuotes (TPT)
RatingPlans (1) ---< RatingFactors
PostalRisks: jurisdiction + dataset version + postal code
```

---

## API Endpoints

### Base route

`/api/quotes` on the configured API host.

### Authentication
All endpoints require JWT token in Authorization header:
```
Authorization: Bearer <jwt_token>
```

**Implementation:** JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`) is registered in `Program.cs` and validates tokens issued by the Identity Service:
- **Issuer and audience**: configurable through `Jwt:Issuer` and `Jwt:Audience`; both are `InsurancePortal.IdentityService.Api` in the checked-in settings.
- **Signing key**: Symmetric key from `Jwt:Secret`.
- **Validation**: Issuer, audience, signing key, and lifetime are validated; clock skew is not explicitly configured.
- **Pipeline**: `UseAuthentication()` runs before `UseAuthorization()`; `QuotesController` is protected with `[Authorize]`.

The controller parses the name identifier claim as a GUID for creation, generating an ID when the claim is absent. A present non-GUID claim produces a server error. This fallback does not allow unauthenticated access. There are no per-resource ownership, role or permission checks in this controller; `GET /user/{userId}` accepts the user ID from the route.

### Endpoints

#### 1. Create Quote (POST /api/quotes)
Creates one quote for the specified `productType` (Auto = 1, Home = 2, Health = 3). The corresponding product object and its `location` are required. The location IDs below are illustrative and must match the LocationService installation; the address and rating data must also exist in the relevant systems.

```json
{
  "productType": 1,
  "autoQuote": {
	"location": {
	  "locationTypeId": 1,
	  "streetAddress1": "123 Example Street NE",
	  "city": "Calgary",
	  "postalCode": "T3J 0A1",
	  "provinceId": 1,
	  "countryId": 1
	},
	"vin": "1HGBH41JXMN109186",
	"vehicleMakeId": 1,
	"vehicleModelId": 1,
	"vehicleColorId": 1,
	"year": 2020,
	"mileage": 45000,
	"drivingRecord": "Clean",
	"numberOfDrivers": 1,
	"estimatedAnnualMileage": 15000,
	"vehicleTypeId": 1,
	"atFaultAccidents": 0,
	"speedingTickets": 0
  }
}
```

On success the response is `201 Created` with `{ message, referenceNumber, quote }`. The quote contains its product, location snapshot, premium and, for table-rated Auto/Home, the rating breakdown. The handler sets `StatusId` to Draft, while the response currently reads the separate obsolete `Status` property; the response status must not be assumed to reflect `StatusId`.

**Error Responses:**
- `400 Bad Request`: Invalid input data
- `401 Unauthorized`: Missing/invalid JWT token
- `500 Internal Server Error`: Server error

---

#### 2. Get Quote (GET /api/quotes/{id})
Retrieve a specific quote with all product details.

**Request:**
```http
GET /api/quotes/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):** A `QuoteResponse` with product details and, when available, the stored rating breakdown. The response status caveat described above also applies.

**Error Responses:**
- `404 Not Found`: Quote does not exist
- `401 Unauthorized`: Missing/invalid JWT token

---

#### 3. Get User Quotes (GET /api/quotes/user/{userId})
Retrieve all quotes for a specific user.

**Request:**
```http
GET /api/quotes/user/660e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):** An array of `QuoteResponse` objects.

---

#### 4. Accept Quote (POST /api/quotes/{id}/accept)
Marks a non-expired quote as converted and persists it. The handler does not create a policy or publish an event.

**Request:**
```http
POST /api/quotes/550e8400-e29b-41d4-a716-446655440000/accept
```

**Response (200 OK):** The updated `QuoteResponse`; its `status` is mapped from the obsolete `Status` property, not `StatusId`.

**Side Effects:**
- The domain updates `StatusId` to `(int)QuoteStatus.Converted` and persists the quote.
- No downstream events are published by this handler.

**Error Responses:**
- `400 Bad Request`: Quote is expired
- `404 Not Found`: Quote does not exist
- `401 Unauthorized`: Missing/invalid JWT token

---

#### 5. Cancel Quote (POST /api/quotes/{id}/cancel)
Cancel a quote and mark it as expired.

**Request:**
```http
POST /api/quotes/550e8400-e29b-41d4-a716-446655440000/cancel
```

**Response (200 OK):** The updated `QuoteResponse`; the response status caveat applies.

---

#### 6. Get Expired Quotes (GET /api/quotes/expired)
Retrieves quotes past `ValidUntil`, regardless of their status ID. The controller does not enforce an admin role.

**Request:**
```http
GET /api/quotes/expired
```

**Response (200 OK):** An array of `QuoteResponse` objects.

---

#### 7. Expire Old Quotes (POST /api/quotes/expire-old)
Marks old quotes as expired; the controller does not enforce an admin role.

**Request:**
```http
POST /api/quotes/expire-old
```

**Response (200 OK):**
```json
{
  "message": "Old quotes have been marked as expired"
}
```

The endpoint is available for an external scheduler, but no scheduler is implemented or registered here.

---

## CQRS Architecture

The Quote Service separates writes (commands) from reads (queries). The former `IQuoteService` facade has been removed; each use case is a dedicated message type with its own handler.

### Dispatch Flow

1. `QuotesController` builds a command or query record.
2. It calls `IDispatcher.SendAsync<TCommand, TResult>` or `IDispatcher.QueryAsync<TQuery, TResult>`.
3. `Dispatcher` (Infrastructure) resolves `ICommandHandler<,>` or `IQueryHandler<,>` from DI and invokes `HandleAsync`.

No third-party mediator package is used. A missing handler registration throws `InvalidOperationException` with the message type name.

### Commands (state changes)

| Command | Result | Handler | Endpoint |
|---------|--------|---------|----------|
| `CreateQuoteCommand(UserId, Request, AccessToken)` | `Quote` | `CreateQuoteCommandHandler` | POST /api/quotes |
| `AcceptQuoteCommand(QuoteId)` | `Quote?` | `AcceptQuoteCommandHandler` | POST /api/quotes/{id}/accept |
| `CancelQuoteCommand(QuoteId)` | `Quote?` | `CancelQuoteCommandHandler` | POST /api/quotes/{id}/cancel |
| `ExpireOldQuotesCommand()` | `Unit` | `ExpireOldQuotesCommandHandler` | POST /api/quotes/expire-old |

`CreateQuoteCommand` carries the bearer token so the handler can forward it to LocationService.

### Queries (reads)

| Query | Result | Handler | Endpoint |
|-------|--------|---------|----------|
| `GetQuoteByIdQuery(QuoteId)` | `Quote?` | `GetQuoteByIdQueryHandler` | GET /api/quotes/{id} |
| `GetUserQuotesQuery(UserId)` | `IEnumerable<Quote>` | `GetUserQuotesQueryHandler` | GET /api/quotes/user/{userId} |
| `GetExpiredQuotesQuery()` | `IEnumerable<Quote>` | `GetExpiredQuotesQueryHandler` | GET /api/quotes/expired |

### Registration

`Program.cs` registers `IDispatcher` to `Dispatcher` and every handler as scoped. `IQuoteRepository` and the typed `ILocationClient` are registered alongside them. Handlers receive their dependencies through constructor injection.

### Status

- Migrated: all seven existing endpoints.
- Removed: `IQuoteService`, `QuoteService`, `QuoteServiceWithEvents`.
- Behavior is unchanged: routes, status codes, validation rules, and response mapping are preserved.

---

## Design Patterns

### 1. **Aggregate Root Pattern (DDD)**
- **Quote** is the aggregate root
- All product quotes must be accessed through Quote
- Ensures consistency of bundled quotes
- Centralizes business logic for multi-product operations

### 2. **Table-Per-Type (TPT) Inheritance**
- Common offer data is stored in `ProductQuotes`.
- `AutoQuotes`, `HomeQuotes` and `HealthQuotes` contain product-specific columns and share the base ID.
- Retrieving product details involves joins to the derived tables.

### 3. **Value Objects (DDD)**
- HomeQuoteDetails, AutoQuoteDetails, HealthQuoteDetails
- Immutable and validated at construction
- Encapsulate product-specific logic
- Compared by value, not identity
- Owned types in EF Core (stored with parent entity)

### 4. **Repository Pattern**
- IQuoteRepository interface separates data access logic
- Concrete implementation in Infrastructure layer
- Supports unit testing with mock repositories
- Centralizes database queries

### 5. **CQRS (Command Query Responsibility Segregation)**
- Commands (state changes) and queries (reads) are separate types, each with a dedicated handler
- Controllers dispatch through IDispatcher instead of an application service facade
- Each handler owns one use case and depends only on the repository/client it needs
- See [CQRS Architecture](#cqrs-architecture)

### 6. **DTO Pattern**
- CreateQuoteRequest for API input
- QuoteResponse for API output
- Separates internal domain from external API contract
- Allows versioning without domain changes

### 7. **Template Method Pattern**
- ProductQuote abstract base class defines algorithm
- Subclasses (HomeQuote, AutoQuote, HealthQuote) provide implementations
- Validate() and CalculatePremium() are overridden per product

---

## Project Structure

```
InsurancePortal.QuoteService/
│
├── InsurancePortal.QuoteService.API/
│   ├── Controllers/
│   │   └── QuotesController.cs         ([Authorize] REST endpoints under api/quotes)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json                (Configuration, Jwt settings)
│   ├── appsettings.Development.json
│   ├── InsurancePortal.QuoteService.API.http
│   └── Program.cs                      (DI, JWT authentication, Swagger, CORS, migrations)
│
├── InsurancePortal.QuoteService.Application/
│   ├── Clients/
│   │   └── ILocationClient.cs          (LocationService client abstraction)
│   ├── DTOs/
│   │   ├── QuoteDtos.cs
│   │   ├── Requests/
│   │   │   ├── CreateQuoteRequest.cs
│   │   │   ├── CreateHomeQuoteRequest.cs
│   │   │   ├── CreateAutoQuoteRequest.cs
│   │   │   ├── CreateHealthQuoteRequest.cs
│   │   │   └── LocationRequest.cs
│   │   └── Responses/
│   │       ├── QuoteResponse.cs
│   │       ├── HomeQuoteResponse.cs
│   │       ├── AutoQuoteResponse.cs
│   │       ├── HealthQuoteResponse.cs
│   │       └── LocationResponse.cs
│   ├── EventPublishing/
│   │   └── IEventPublisher.cs          (Event publishing abstraction)
│   ├── Repositories/
│   │   └── IQuoteRepository.cs         (Interface)
│   ├── Abstractions/
│   │   └── Cqrs/
│   │       ├── ICommand.cs             (Command marker)
│   │       ├── IQuery.cs               (Query marker)
│   │       ├── ICommandHandler.cs      (Command handler contract)
│   │       ├── IQueryHandler.cs        (Query handler contract)
│   │       ├── IDispatcher.cs          (Dispatch abstraction)
│   │       └── Unit.cs                 (Void result)
│   ├── Quotes/
│   │   ├── Commands/
│   │   │   └── Create, Accept, Cancel and ExpireOld command and handler files
│   │   └── Queries/                    (GetById, GetUser and GetExpired query files)
│   └── Rating/                         (Rating contracts and results)
│
├── InsurancePortal.QuoteService.Domain/
│   ├── Entities/
│   │   ├── Quote.cs                    (Aggregate root)
│   │   ├── ProductQuote.cs             (Abstract base)
│   │   ├── HomeQuote.cs                (Inheritance)
│   │   ├── AutoQuote.cs                (Inheritance)
│   │   ├── HealthQuote.cs              (Inheritance)
│   │   └── Lookups/                    (Lookup entities, int IDs)
│   ├── Enums/                          (Lookup and product enums)
│   ├── Events/
│   │   ├── DomainEvent.cs
│   │   ├── QuoteCreatedEvent.cs
│   │   ├── QuoteAcceptedEvent.cs
│   │   └── QuoteExpiredEvent.cs
│   ├── Utilities/
│   │   └── LookupEnumExtensions.cs
│   ├── ValueObjects/
│   │   ├── HomeQuoteDetails.cs
│   │   ├── AutoQuoteDetails.cs
│   │   └── HealthQuoteDetails.cs
│   └── Class1.cs                       (Placeholder)
│
└── InsurancePortal.QuoteService.Infrastructure/
    ├── Clients/
    │   └── LocationClient.cs           (HTTP client for LocationService)
    ├── Cqrs/
    │   └── Dispatcher.cs               (Resolves command/query handlers from DI)
    ├── Data/
    │   ├── QuoteDbContext.cs           (EF DbContext)
    │   ├── QuoteDbContextFactory.cs
    │   ├── Configurations/
    │   │   ├── QuoteConfiguration.cs
    │   │   ├── ProductQuoteConfiguration.cs
    │   │   ├── HomeQuoteConfiguration.cs
    │   │   ├── AutoQuoteConfiguration.cs
    │   │   ├── HealthQuoteConfiguration.cs
    │   │   └── Lookups/                (Lookup table configurations)
    │   └── Seed/
    │       └── SeedData.cs
    ├── EventPublishing/
    │   └── ServiceBusEventPublisher.cs (Azure Service Bus publisher)
    ├── Migrations/                     (Initial schema, vehicle lookups, rating and postal-code changes)
    ├── Repositories/
    │   └── QuoteRepository.cs          (Implementation)
    ├── Services/
    │   └── BasicRatingEngine.cs         (Configured Auto and Home rating)
    └── QuoteService.md                 (This file)
```

---

## Service Layer Design

The use cases are implemented by CQRS handlers rather than a `QuoteService` class.

### Quote creation

1. The controller derives the user ID from the name identifier claim when present and forwards the bearer token.
2. `CreateQuoteCommandHandler` validates the request and product type, creates a new quote and reference number, and calls LocationService with the submitted address.
3. It creates the selected product and snapshots the returned location. Auto and Home use `IRatingEngine` when enabled; otherwise the domain calculation is used. Health always uses the domain calculation.
4. The aggregate validates the product, sets its premium and validity, recalculates the total, and the repository saves the quote. The location request precedes rating and quote persistence; a later failure does not roll back the external location.

### Other operations

- Accept loads the quote, checks expiry, sets its status ID using `QuoteStatus.Converted`, and saves it; it does not create a policy or publish events.
- Cancel sets the status ID using `QuoteStatus.Expired` and saves it.
- Expire-old selects quotes past their validity regardless of lifecycle state, then saves each whose status ID differs from `QuoteStatus.Expired` individually; this can include previously converted quotes. No scheduler is registered.
- `QuoteStatus` and `QuoteStatusEnum` have different numeric meanings. The handlers use both, and response mapping uses obsolete `Status` rather than `StatusId`; do not assume the response status or lookup name reflects the lifecycle operation until this is reconciled.

---

## Implementation Notes

### Current Status

**Implemented:**
- Domain model, EF Core configuration and repository
- CQRS handlers, REST controller and dependency injection
- Configurable table-driven Auto and Home rating
- JWT Bearer authentication (tokens issued by IdentityService)

**Not yet integrated:**
- ServiceBusEventPublisher is implemented but not registered or invoked.

**Potential enhancements:**
- Event publishing to Azure Service Bus
- Unit tests for domain and service layer
- Integration tests for API endpoints
- Configuration management (appsettings by environment)
- Error handling middleware
- Logging and monitoring
- API versioning
- Rate limiting

### Key Implementation Decisions

1. **Value Object Storage**
   - **Decision**: Use owned types in EF Core (flatten to parent table)
   - **Purpose**: Store product details with the derived product entity while retaining value-object semantics.

2. **Premium Calculation**
   - **Legacy**: Auto and Home use simplified domain calculations when basic rating is disabled; Health always uses its domain calculation.
   - **Configured**: [Table-driven Auto and Home rating](#table-driven-rating).
   - **Operational requirement**: Supply and validate approved rating and risk data; the engine does not establish regulatory approval.

3. **Reference Number Format**
   - **Format**: QT-YYYYMMDD-XXXXX
   - **Rationale**: Human-readable, time-sortable, includes random component

4. **Quote Validity Period**
   - **Duration**: 30 days, hard-coded in the `Quote` constructor.
   - **Configuration**: No current setting changes this duration.

5. **Bundled Quotes**
   - **Design**: One Quote can contain multiple products
   - **Constraint**: Max one quote per product type per bundle
   - **Current API**: Creates one product per quote; no bundle discounts are implemented.

---

## Table-Driven Rating

### Configuration and migration

`BasicRating:Enabled` controls Auto and Home rating in all environments; the checked-in setting is enabled. When disabled, Auto and Home use the existing hard-coded calculations; Health always uses its existing calculation. Apply EF migrations and supply independently approved rate plans, factors and postal-risk data before enabling the engine. There is no administration API; these tables must be maintained externally. Missing or ambiguous plans, risk records, factors or currencies cause an explicit error instead of falling back to legacy pricing. No new demonstration data is seeded by the current model.

`ConfigureRatingJurisdiction` preserves existing rows, deactivates the two originally named sample plans, and initializes their new jurisdiction, currency and risk version fields to zero. Postal-risk jurisdiction fields are also initialized to zero. These old sample rates and risk levels are fabricated and remain in previously migrated databases for review, not for rating. Existing independently maintained rows likewise need their new fields populated before use. The original migration no longer inserts sample data on a fresh database; previously applied migrations are not rerun. The new migration does not delete potentially customized data. A rollback does not automatically republish the old plans.

The prior conversion block was removed: any existing unexpired quote, including one previously calculated using sample data, can now be converted. Review or expire such quotes before enabling policy conversion. Existing premiums and snapshots are not automatically recalculated.

### Data and calculation

- Each `RatingPlans` row is a versioned plan with product, LocationService country and province IDs, currency lookup ID, postal-risk dataset version, base rate, publication status, and half-open UTC effective dates. Exactly one published plan must match the product and jurisdiction. The plan is annual (12 months); Auto uses a fixed base and Home a percentage of home value. Currency must exist in the currency lookup and have two decimal places.
- `RatingFactors` contains inclusive integer bands and multipliers. Auto applies vehicle type, accident count, ticket count, hail, theft and crash factors; Home applies hail, theft and fire. Exactly one positive multiplier must match each input. Decimal calculations round only the final amount to two decimals, midpoint away from zero.
- `PostalRisks` is maintained separately, keyed by country, province, plan-selected dataset version and postal code. `PostalCode` is `nvarchar(16)`. Rating uppercases the quote's postal code but does not remove spaces; store codes in a matching format (for example, `T3J 0A1` must match `T3J 0A1`). Each record supplies hail, theft, fire and crash levels. Provide approved classifications for every supported postal code; there is no hard-coded postal-code whitelist.
- Auto requests must provide `vehicleTypeId`, `atFaultAccidents` and `speedingTickets`; zero counts must be explicit. The vehicle type must match an applicable configured factor. Counts are aggregate totals for all drivers for the applicable plan's externally defined lookback period; the caller is responsible for aggregation and verification. There is no per-driver calculation, lookback enforcement or claim-history verification.
- `ProductQuotes.RatingSnapshot` retains the plan, base, inputs, factors, intermediate amounts, jurisdiction, dataset version, currency and premium. Responses expose it as `rating`. Snapshot-bearing quotes may be converted. Historical snapshots with the earlier currency format remain readable.

The engine does not enforce immutable published plans or risk records, provide an approval workflow, support fixed adjustments or coverage-level rates, validate geographic data against an authoritative source, or provide bundle discounts. The feature switch and the older Auto/Home/Health calculations are not a substitute for approved production rates. Existing Auto mileage and Home security/age adjustments are not part of this table-driven calculation.

### Rating implementation and verification

`CreateQuoteCommandHandler` calls `IRatingEngine`; `BasicRatingEngine` separates the rating process into these steps:

1. `GetRatingLocation` validates the product and, for Auto, requires a positive vehicle type and non-negative accident and ticket counts. It requires positive country and province IDs and a postal code, then uppercases the postal code without removing spaces.
2. `GetEffectivePlanAsync` selects exactly one published plan for the product and jurisdiction at the current UTC time. Its start is inclusive and its end is exclusive. The base rate and risk dataset version must be positive; Home must use a percentage of home value and Auto a fixed base.
3. `GetCurrencyAsync` requires an existing two-decimal currency. `GetPostalRiskAsync` matches the uppercased postal code, jurisdiction and the plan's risk dataset version using database equality (subject to SQL collation); it does not strip spaces.
4. `CalculateBasePremium` calculates the fixed Auto base or Home value multiplied by its base rate. `GetFactorInputs` assembles the ordered product and geographic inputs described above.
5. `ApplyFactors` and `FindFactor` require exactly one matching inclusive band with a positive multiplier for each input. They multiply in order and retain each unrounded intermediate amount. The engine rounds only the final premium to two decimal places, midpoint away from zero.
6. The result records the plan, location, risk dataset, currency, 12-month term, base, applied factors and premium. The aggregate stores the product premium and snapshot and recomputes the quote total.

Disabled rating and Health return no table-driven result so that the existing calculation remains in use. Validation or configuration failures are reported; they do not silently select a factor or fall back. `BasicRatingEngineTests` use explicit test-only data. The existing test project targets .NET 8 while application projects target .NET 10 and requires modernization to run as a full suite.

---

## Future Enhancements

### Phase 2: Event-Driven Architecture
- [ ] Integrate Azure Service Bus for event publishing
- [ ] Implement QuoteCreated, QuoteAccepted, QuoteExpired events
- [ ] Subscribe to UserCreated events from User Service
- [ ] Dead-letter queue handling for failed events

### Phase 3: AI/ML Integration
- [ ] Replace premium calculation with AI service calls
- [ ] Real-time risk assessment
- [ ] Fraud detection system
- [ ] Recommendation engine for coverage suggestions

### Phase 4: Advanced Features
- [ ] Quote comparison endpoint (multiple quotes)
- [ ] Quote sharing (send to family/friends)
- [ ] Saved preferences (remember user inputs)
- [ ] Multi-language support
- [ ] Quote PDF generation
- [ ] Quote history with version control

### Phase 5: Resilience & Performance
- [ ] Distributed caching (Redis)
- [ ] API rate limiting
- [ ] Circuit breaker pattern for downstream calls
- [ ] Database query optimization and indexing
- [ ] Background job processing (Hangfire)
- [ ] Audit logging for compliance

### Phase 6: Policy Integration
- [ ] Automatic policy creation from accepted quote
- [ ] Same policy number if products are bundled
- [ ] Premium alignment between quote and policy
- [ ] Automatic renewal quote generation

---

## Configuration

The API reads `ConnectionStrings:DefaultConnection`, `LocationService:BaseUrl`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Secret`, and `BasicRating:Enabled`. Configure deployment-specific values and provide a secure signing key outside source control. The checked-in rating switch is enabled. Quote validity and reference prefix are hard-coded, not `QuoteSettings` options. The unused Service Bus publisher is not registered at startup.

---

## Testing Strategy

The repository contains xUnit tests, including `BasicRatingEngineTests`. The test project targets .NET 8, references .NET 10 projects and is not included in the solution; it needs modernization before it can run as the normal solution test suite. API integration, lifecycle status consistency, LocationService failure handling and rating configuration are important areas for further testing. Event publication and API bundle creation are not currently implemented and cannot be tested as existing flows.

---

## Deployment

### Development

With the .NET 10 SDK and the appropriate SQL Server and LocationService configuration:

```powershell
dotnet build .\InsurancePortal.QuoteService.slnx
dotnet run --project .\InsurancePortal.QuoteService.API\InsurancePortal.QuoteService.API.csproj
```

### Database setup

The API calls `Database.Migrate()` on startup using its configured connection. EF CLI operations use `QuoteDbContextFactory`, whose design-time connection string is separate from the API configuration; check its target before running a database update. To generate or apply migrations, use the Infrastructure project and API startup project with a compatible `dotnet-ef` tool. Configure approved rating data independently as described above.

---

## Related Services

- **IdentityService**: Issues JWTs validated by QuoteService.
- **LocationService**: Creates and returns product address records over HTTP.
- **Policy, Claims, Billing and Notification services**: Possible future integrations; no quote event is currently published to them.

---

## References

- [Domain-Driven Design (DDD)](https://www.domainlanguage.com/ddd/)
- [EF Core inheritance mapping](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [Azure Service Bus Messaging](https://azure.microsoft.com/en-us/services/service-bus/)
- [REST API Design Best Practices](https://restfulapi.net/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

## Document Scope

This document consolidates the service architecture, current API and rating implementation. Source code and EF migrations remain authoritative for implementation details.
