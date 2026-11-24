# Insurance Portal - Quote Service Architecture & Design

## Project Overview

**Quote Service** is a microservice for the Insurance Portal that handles quote generation for multiple insurance products (Home, Auto, Health) with support for bundled quotes. This document captures the complete architecture, design decisions, and implementation details using Spec-Driven Development (SDD) approach.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain Model Design](#domain-model-design)
3. [Database Design](#database-design)
4. [API Endpoints](#api-endpoints)
5. [Design Patterns](#design-patterns)
6. [Project Structure](#project-structure)
7. [CQRS Architecture](#cqrs-architecture)
8. [Implementation Notes](#implementation-notes)
9. [Table-Driven Rating](#table-driven-rating)
10. [Future Enhancements](#future-enhancements)

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────┐
│           API Gateway / User Service        │
│  (Authentication via JWT Tokens)            │
└──────────────────┬──────────────────────────┘
				   │
		┌──────────▼──────────┐
		│                     │
	┌───▼────────┐    ┌──────▼──────┐
	│ Quote      │    │ Azure       │
	│ Service    │    │ Service Bus │
	│ API        │    │ (Events)    │
	└───┬────────┘    └──────┬──────┘
		│                    │
	┌───▼────────────────────▼──────┐
	│   SQL Database                │
	│   (Quotes, ProductQuotes)      │
	└───────────────────────────────┘
		│
		└──► Quote Events:
			 - QuoteCreated
			 - QuoteAccepted
			 - QuoteExpired
			 - QuoteUpdated
```

### Microservices Integration

- **User Service**: Provides authentication and authorization via JWT tokens
- **Policy Service**: Consumes `QuoteAccepted` events to create policies
- **Billing Service**: Consumes `QuoteAccepted` events for invoicing
- **Notification Service**: Consumes quote events for email/SMS alerts

---

## Domain Model Design

> The [table-driven rating engine](#table-driven-rating) is described below. The calculations in this section describe the legacy path when table-driven rating is disabled.

### Design Pattern: Table-Per-Hierarchy (TPH) with Composition

The Quote Service uses a sophisticated domain model supporting multiple insurance products with different requirements, all within a single quote entity.

### Key Domain Entities

#### 1. **Quote (Aggregate Root)**
Represents a complete quote request that can contain multiple product quotes.

**Properties:**
- `Id`: Unique identifier (GUID)
- `UserId`: Foreign key to User Service
- `Status`: QuoteStatus enum (Active, Expired, Converted, Rejected)
- `CreatedAt`: Timestamp of quote creation
- `ValidUntil`: Expiration date, set to 30 days from creation (`DateTime.UtcNow.AddDays(30)`)
- `TotalPremium`: Sum of all product premiums
- `ReferenceNumber`: Unique human-readable reference (QT-YYYYMMDD-XXXXX)
- `ProductQuotes`: Collection of product-specific quotes

**Key Methods:**
```csharp
void AddProductQuote(ProductQuote quote)  // Add product to bundle
void RemoveProductQuote(ProductType type) // Remove product from bundle
void Validate()                           // Validate entire quote
void MarkAsConverted()                    // Convert to policy
void MarkAsExpired()                      // Mark as expired
bool IsExpired                            // Check expiration status
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
- `Discriminator`: TPH discriminator column

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
		homeAddress: "123 Main St, Springfield",
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
- Age (40-50): +25%
- Age (>50): +50%
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
	public string HomeAddress { get; }
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

**QuoteStatus**
- `Active`: Quote is valid and can be accepted
- `Expired`: Quote has passed ValidUntil date
- `Converted`: Quote has been accepted and converted to policy
- `Rejected`: Quote was rejected by user or system

**ProductType**
- `Home`: Home insurance (value = 1)
- `Auto`: Auto insurance (value = 2)
- `Health`: Health insurance (value = 3)

---

## Database Design

### Technology Stack
- **Database**: SQL Server
- **ORM**: Entity Framework Core 10.0
- **Pattern**: Table-Per-Hierarchy (TPH) for inheritance

### Database Schema

#### Quotes Table
```sql
CREATE TABLE [Quotes] (
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[UserId] UNIQUEIDENTIFIER NOT NULL,
	[Status] INT NOT NULL,
	[CreatedAt] DATETIME2 NOT NULL,
	[ValidUntil] DATETIME2 NOT NULL,
	[TotalPremium] DECIMAL(18,2) NOT NULL,
	[ReferenceNumber] NVARCHAR(50)
);

CREATE INDEX [IX_Quotes_UserId] ON [Quotes]([UserId]);
CREATE INDEX [IX_Quotes_Status] ON [Quotes]([Status]);
CREATE INDEX [IX_Quotes_CreatedAt] ON [Quotes]([CreatedAt]);
```

#### ProductQuotes Table (TPH)
```sql
CREATE TABLE [ProductQuotes] (
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[QuoteId] UNIQUEIDENTIFIER NOT NULL,
	[Discriminator] NVARCHAR(50) NOT NULL,  -- HomeQuote, AutoQuote, HealthQuote
	[QuotedPremium] DECIMAL(18,2) NOT NULL,
	[ValidUntil] DATETIME2 NOT NULL,
	[CreatedAt] DATETIME2 NOT NULL,

	-- HomeQuote Columns
	[HomeAddress] NVARCHAR(500),
	[SquareFootage] INT,
	[YearBuilt] INT,
	[HomeType] NVARCHAR(50),
	[HomeValue] DECIMAL(18,2),
	[HasSecuritySystem] BIT,
	[HasFireAlarm] BIT,
	[NumberOfOccupants] INT,

	-- AutoQuote Columns
	[VIN] NVARCHAR(20),
	[VehicleMakeId] INT NOT NULL,   -- FK -> VehicleMakeLookups
	[VehicleModelId] INT NOT NULL,  -- FK -> VehicleModelLookups
	[VehicleYear] INT,
	[VehicleColorId] INT NOT NULL,  -- FK -> VehicleColorLookups
	[Mileage] INT,
	[DrivingRecord] NVARCHAR(50),
	[NumberOfDrivers] INT,
	[EstimatedAnnualMileage] DECIMAL(18,2),

	-- HealthQuote Columns
	[Age] INT,
	[HealthStatus] NVARCHAR(50),
	[HasPreexistingConditions] BIT,
	[Smoker] BIT,
	[CoverageType] NVARCHAR(50),
	[Dependents] INT,

	FOREIGN KEY ([QuoteId]) REFERENCES [Quotes]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_ProductQuotes_QuoteId] ON [ProductQuotes]([QuoteId]);
```

### Entity Relationship Diagram

```
┌─────────────────┐
│     Quotes      │
├─────────────────┤
│ Id (PK)         │
│ UserId          │
│ Status          │
│ CreatedAt       │
│ ValidUntil      │
│ TotalPremium    │
│ ReferenceNumber │
└────────┬────────┘
		 │
		 │ 1 to Many
		 │
	┌────▼─────────────────┐
	│   ProductQuotes      │
	├──────────────────────┤
	│ Id (PK)              │
	│ QuoteId (FK)         │
	│ Discriminator (TPH)  │
	│ QuotedPremium        │
	│ ValidUntil           │
	│ CreatedAt            │
	│                      │
	│ [Product-Specific]   │
	│ HomeAddress (Home)   │
	│ VIN (Auto)           │
	│ Age (Health)         │
	└──────────────────────┘
```

---

## API Endpoints

### Base URL
```
https://api.insuranceportal.com/api/quotes
```

### Authentication
All endpoints require JWT token in Authorization header:
```
Authorization: Bearer <jwt_token>
```

**Implementation:** JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`) is registered in `Program.cs` and validates tokens issued by the Identity Service:
- **Issuer**: `InsurancePortal.IdentityService.Api` (`Jwt:Issuer`)
- **Audience**: `InsurancePortal.QuoteService.Api` (`Jwt:Audience`)
- **Signing key**: Symmetric key from `Jwt:Secret` (HMAC-SHA256)
- **Validation**: Issuer, audience, signing key, and lifetime are validated; clock skew is zero
- **Pipeline**: `UseAuthentication()` runs before `UseAuthorization()`; `QuotesController` is protected with `[Authorize]`

### Endpoints

#### 1. Create Quote (POST /api/quotes)
Creates a new bundled quote with one or more products.

**Request:**
```http
POST /api/quotes?userId={userId}
Content-Type: application/json

{
  "homeQuote": {
	"homeAddress": "123 Main St, Springfield",
	"squareFootage": 2500,
	"yearBuilt": 1995,
	"homeType": "Single Family",
	"homeValue": 450000,
	"hasSecuritySystem": true,
	"hasFireAlarm": true,
	"numberOfOccupants": 4
  },
  "autoQuote": {
	"vin": "1HGBH41JXMN109186",
	"vehicleMakeId": 1,
	"vehicleModelId": 1,
	"year": 2018,
	"vehicleColorId": 6,
	"mileage": 45000,
	"drivingRecord": "Clean",
	"numberOfDrivers": 2,
	"estimatedAnnualMileage": 15000
  },
  "healthQuote": null
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440000",
  "referenceNumber": "QT-20261011-ABC12",
  "status": "Active",
  "createdAt": "2026-10-11T10:30:00Z",
  "validUntil": "2026-11-10T10:30:00Z",
  "totalPremium": 3750.50,
  "homeQuote": {
	"id": "750e8400-e29b-41d4-a716-446655440000",
	"homeAddress": "123 Main St, Springfield",
	"squareFootage": 2500,
	"yearBuilt": 1995,
	"homeType": "Single Family",
	"homeValue": 450000,
	"hasSecuritySystem": true,
	"hasFireAlarm": true,
	"numberOfOccupants": 4,
	"quotedPremium": 2112.75,
	"validUntil": "2026-11-10T10:30:00Z"
  },
  "autoQuote": {
	"id": "850e8400-e29b-41d4-a716-446655440000",
	"vin": "1HGBH41JXMN109186",
	"make": "Honda",
	"model": "Civic",
	"year": 2018,
	"color": "Blue",
	"mileage": 45000,
	"drivingRecord": "Clean",
	"numberOfDrivers": 2,
	"estimatedAnnualMileage": 15000,
	"quotedPremium": 1637.75,
	"validUntil": "2026-11-10T10:30:00Z"
  },
  "healthQuote": null
}
```

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

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440000",
  "referenceNumber": "QT-20261011-ABC12",
  "status": "Active",
  "createdAt": "2026-10-11T10:30:00Z",
  "validUntil": "2026-11-10T10:30:00Z",
  "totalPremium": 3750.50,
  "homeQuote": {...},
  "autoQuote": {...},
  "healthQuote": null
}
```

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

**Response (200 OK):**
```json
[
  {
	"id": "550e8400-e29b-41d4-a716-446655440000",
	"userId": "660e8400-e29b-41d4-a716-446655440000",
	"referenceNumber": "QT-20261011-ABC12",
	"status": "Active",
	...
  },
  {
	"id": "560e8400-e29b-41d4-a716-446655440000",
	"userId": "660e8400-e29b-41d4-a716-446655440000",
	"referenceNumber": "QT-20261010-XYZ99",
	"status": "Expired",
	...
  }
]
```

---

#### 4. Accept Quote (POST /api/quotes/{id}/accept)
Accept a quote and convert it to a policy. This triggers downstream events.

**Request:**
```http
POST /api/quotes/550e8400-e29b-41d4-a716-446655440000/accept
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Converted",
  ...
}
```

**Side Effects:**
- Quote status changed to `Converted`
- `QuoteAccepted` event published to Service Bus
- Policy Service receives event and creates policy
- Billing Service receives event and creates invoice
- Notification Service receives event and sends confirmation email

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

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Expired",
  ...
}
```

---

#### 6. Get Expired Quotes (GET /api/quotes/expired)
Admin endpoint to retrieve all expired quotes.

**Request:**
```http
GET /api/quotes/expired
```

**Response (200 OK):**
```json
[
  {...},
  {...}
]
```

---

#### 7. Expire Old Quotes (POST /api/quotes/expire-old)
Admin/scheduled task endpoint to mark all old quotes as expired.

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

**Used By:**
- Scheduled Azure Function (runs daily)
- Background job service

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

### 2. **Table-Per-Hierarchy (TPH) Inheritance**
- Single ProductQuotes table for all product types
- Discriminator column determines product type
- Efficient queries for single product type
- Simplifies relationship management
- Trade-off: Some NULL columns for each product type (acceptable)

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
│   │   │   ├── CreateQuoteCommand.cs       (Command + handler)
│   │   │   ├── AcceptQuoteCommand.cs       (Command + handler)
│   │   │   ├── CancelQuoteCommand.cs       (Command + handler)
│   │   │   └── ExpireOldQuotesCommand.cs   (Command + handler)
│   │   └── Queries/
│   │       ├── GetQuoteByIdQuery.cs       (Query + handler)
│   │       ├── GetUserQuotesQuery.cs      (Query + handler)
│   │       └── GetExpiredQuotesQuery.cs   (Query + handler)
│   └── Class1.cs                       (Placeholder)
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
    ├── Migrations/
    │   ├── 20261120213958_InitialQuotes.cs
    │   ├── 20261120213958_InitialQuotes.Designer.cs
    │   └── QuoteDbContextModelSnapshot.cs
    ├── Repositories/
    │   └── QuoteRepository.cs          (Implementation)
    └── QuoteService.md                 (This file)
```

---

## Service Layer Design

### QuoteService Class

**Purpose**: Orchestrates quote operations and implements business logic

**Key Methods:**

#### CreateQuoteAsync(Guid userId, CreateQuoteRequest request)
- Validates input (at least one product must be provided)
- Creates domain entities from DTOs
- Constructs ProductQuotes based on request
- Adds all products to Quote aggregate
- Validates complete quote before saving
- Generates unique reference number
- Persists to database

**Pseudo-code:**
```
1. Validate userId and request
2. Create new Quote(userId)
3. If HomeQuote in request:
   - Create HomeQuoteDetails from request
   - Create HomeQuote entity
   - Add to Quote via AddProductQuote()
4. If AutoQuote in request:
   - Create AutoQuoteDetails from request
   - Create AutoQuote entity
   - Add to Quote via AddProductQuote()
5. If HealthQuote in request:
   - Create HealthQuoteDetails from request
   - Create HealthQuote entity
   - Add to Quote via AddProductQuote()
6. Call Quote.Validate() to ensure quote is valid
7. Generate reference number (QT-YYYYMMDD-XXXXX)
8. Call repository.AddAsync(quote)
9. Return quote
```

#### AcceptQuoteAsync(Guid quoteId)
- Retrieves quote by ID
- Validates quote is not expired
- Marks quote as Converted
- Publishes QuoteAccepted event (future)
- Persists changes to database
- Returns updated quote

**Future: Event Publishing**
```
QuoteAccepted event published to Azure Service Bus:
{
  "quoteId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440000",
  "products": ["Home", "Auto"],
  "totalPremium": 3750.50,
  "timestamp": "2026-10-11T10:35:00Z"
}

Subscribers:
- Policy Service: Creates Policy with same products and reference
- Billing Service: Creates Invoice
- Notification Service: Sends confirmation email
```

#### ExpireOldQuotesAsync()
- Queries all expired quotes
- Marks those not yet expired with Expired status
- Batch updates database
- Called by scheduled task

---

## Implementation Notes

### Current Status (Spec-Driven Development Phase)

✅ **Completed:**
- Domain model design (Quote, ProductQuote hierarchy, Value Objects)
- Database schema and EF Core configuration
- Repository pattern implementation
- Application service layer
- REST API controller with all endpoints
- Dependency injection setup
- JWT Bearer authentication (tokens issued by IdentityService)
- Build compilation successful

⏳ **In Progress:**
- ServiceBusEventPublisher implemented (not yet registered in DI)

📋 **Planned:**
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
   - **Rationale**: Simpler schema, better performance, maintains DDD value object semantics
   - **Note**: Currently exploring JSON serialization alternative if needed

2. **Premium Calculation**
   - **Legacy**: Simplified calculations in domain entities when basic rating is disabled.
   - **Configured**: [Table-driven Auto and Home rating](#table-driven-rating).
   - **Operational requirement**: Supply and validate approved rating and risk data; the engine does not establish regulatory approval.

3. **Reference Number Format**
   - **Format**: QT-YYYYMMDD-XXXXX
   - **Rationale**: Human-readable, time-sortable, includes random component

4. **Quote Validity Period**
   - **Duration**: 30 days
   - **Rationale**: Standard practice in insurance
   - **Configurable**: Can be made environment-specific

5. **Bundled Quotes**
   - **Design**: One Quote can contain multiple products
   - **Constraint**: Max one quote per product type per bundle
   - **Rationale**: Supports bundle discounts, common in insurance

---

## Table-Driven Rating

### Configuration and migration

`BasicRating:Enabled` controls Auto and Home rating in all environments; the checked-in setting is enabled. When disabled, Auto and Home use the existing hard-coded calculations; Health always uses its existing calculation. Apply EF migrations and supply independently approved rate plans, factors and postal-risk data before enabling the engine. There is no administration API; these tables must be maintained externally. Missing or ambiguous plans, risk records, factors or currencies cause an explicit error instead of falling back to legacy pricing. No new demonstration data is seeded by the current model.

`ConfigureRatingJurisdiction` preserves existing rows, deactivates the two originally named sample plans, and initializes their new jurisdiction, currency and risk version fields to zero. Postal-risk jurisdiction fields are also initialized to zero. These old sample rates and risk levels are fabricated and remain in previously migrated databases for review, not for rating. Existing independently maintained rows likewise need their new fields populated before use. The original, uncommitted migration no longer inserts sample data on a fresh database; previously applied migrations are not rerun. The new migration does not delete potentially customized data. A rollback does not automatically republish the old plans.

The prior conversion block was removed: any existing unexpired quote, including one previously calculated using sample data, can now be converted. Review or expire such quotes before enabling policy conversion. Existing premiums and snapshots are not automatically recalculated.

### Data and calculation

- Each `RatingPlans` row is a versioned plan with product, LocationService country and province IDs, currency lookup ID, postal-risk dataset version, base rate, publication status, and half-open UTC effective dates. Exactly one published plan must match the product and jurisdiction. The plan is annual (12 months); Auto uses a fixed base and Home a percentage of home value. Currency must exist in the currency lookup and have two decimal places.
- `RatingFactors` contains inclusive integer bands and multipliers. Auto applies vehicle type, accident count, ticket count, hail, theft and crash factors; Home applies hail, theft and fire. Exactly one positive multiplier must match each input. Decimal calculations round only the final amount to two decimals, midpoint away from zero.
- `PostalRisks` is maintained separately, keyed by country, province, plan-selected dataset version and normalized exact postal code. `PostalCode` is `nvarchar(16)`; rating removes spaces and uppercases the quote's postal code, so stored risk codes must use the same normalized format (for example, `T3J0A1`, not `T3J 0A1`). Each record supplies hail, theft, fire and crash levels. Provide approved classifications for every supported postal code; there is no hard-coded postal-code whitelist.
- Auto requests must provide `vehicleTypeId`, `atFaultAccidents` and `speedingTickets`; zero counts must be explicit. The vehicle type must match an applicable configured factor. Counts are aggregate totals for all drivers for the applicable plan's externally defined lookback period; the caller is responsible for aggregation and verification. There is no per-driver calculation, lookback enforcement or claim-history verification.
- `ProductQuotes.RatingSnapshot` retains the plan, base, inputs, factors, intermediate amounts, jurisdiction, dataset version, currency and premium. Responses expose it as `rating`. Snapshot-bearing quotes may be converted. Historical snapshots with the earlier currency format remain readable.

The engine does not enforce immutable published plans or risk records, provide an approval workflow, support fixed adjustments or coverage-level rates, validate geographic data against an authoritative source, or provide bundle discounts. The feature switch and the older Auto/Home/Health calculations are not a substitute for approved production rates. Existing Auto mileage and Home security/age adjustments are not part of this table-driven calculation.

### Rating implementation and verification

`CreateQuoteCommandHandler` calls `IRatingEngine`; `BasicRatingEngine` separates the rating process into these steps:

1. `GetRatingLocation` validates the product and, for Auto, requires a positive vehicle type and non-negative accident and ticket counts. It requires positive country and province IDs and a postal code, then removes spaces and uppercases the postal code.
2. `GetEffectivePlanAsync` selects exactly one published plan for the product and jurisdiction at the current UTC time. Its start is inclusive and its end is exclusive. The base rate and risk dataset version must be positive; Home must use a percentage of home value and Auto a fixed base.
3. `GetCurrencyAsync` requires an existing two-decimal currency. `GetPostalRiskAsync` requires an exact match for the normalized postal code, jurisdiction and the plan's risk dataset version.
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

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=.;Database=InsurancePortal.QuoteService;Trusted_Connection=true;"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft": "Warning"
	}
  },
  "QuoteSettings": {
	"ValidityDays": 30,
	"ReferenceNumberPrefix": "QT"
  },
  "ServiceBus": {
	"ConnectionString": "[Azure Service Bus Connection String]"
  },
  "Jwt": {
	"Secret": "[Shared HMAC signing key, min. 32 characters]",
	"Issuer": "InsurancePortal.IdentityService.Api",
	"Audience": "InsurancePortal.QuoteService.Api"
  }
}
```

---

## Testing Strategy

### Unit Tests
- Domain entity validation (Quote, ProductQuote)
- Value object equality and validation
- Service layer business logic
- Premium calculation logic

### Integration Tests
- Repository CRUD operations
- Database migrations
- EF Core configuration
- Full API endpoint testing

### E2E Tests
- Complete quote creation to acceptance flow
- Event publishing verification
- Multi-product bundle scenarios
- Error handling scenarios

---

## Deployment

### Development
```bash
dotnet build
dotnet run --project InsurancePortal.QuoteService.API
```

### Database Setup
```bash
dotnet ef database update -p InsurancePortal.QuoteService.Infrastructure -s InsurancePortal.QuoteService.API
```

### Docker
```bash
docker build -t insuranceportal.quoteservice:latest .
docker run -p 5000:5000 insuranceportal.quoteservice:latest
```

### Azure Deployment
- Push to Azure Container Registry
- Deploy to Azure App Service or AKS
- Configure managed identity for database access
- Set up Key Vault for secrets
- Configure Application Insights for monitoring

---

## Related Services

- **User Service**: Provides authentication and user context
- **Policy Service**: Consumes quote events to create policies
- **Claims Service**: References policies created from quotes
- **Billing Service**: Creates invoices from accepted quotes
- **Notification Service**: Sends notifications for quote events

---

## References

- [Domain-Driven Design (DDD)](https://www.domainlanguage.com/ddd/)
- [EF Core Table-Per-Hierarchy](https://docs.microsoft.com/en-us/ef/core/modeling/inheritance)
- [Azure Service Bus Messaging](https://azure.microsoft.com/en-us/services/service-bus/)
- [REST API Design Best Practices](https://restfulapi.net/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-10-11 | Architecture Team | Initial specification (Spec-Driven Development) |

---

## Contact & Questions

For questions about this architecture, please refer to:
- Architecture: [Team Email]
- Implementation: [Dev Team Email]
- DevOps: [DevOps Team Email]

---

**Last Updated**: 2026-10-11
**Status**: Spec-Driven Development Phase Complete
