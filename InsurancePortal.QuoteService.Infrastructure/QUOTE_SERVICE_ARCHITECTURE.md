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
7. [Service Layer Design](#service-layer-design)
8. [Implementation Notes](#implementation-notes)
9. [Future Enhancements](#future-enhancements)

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
- `ValidUntil`: Expiration date (default 30 days)
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
	public string Make { get; }
	public string Model { get; }
	public int Year { get; }
	public string Color { get; }
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
	[Make] NVARCHAR(50),
	[Model] NVARCHAR(50),
	[VehicleYear] INT,
	[Color] NVARCHAR(30),
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
	"make": "Honda",
	"model": "Civic",
	"year": 2018,
	"color": "Blue",
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

### 5. **Service Layer Pattern**
- IQuoteService handles business logic
- Orchestrates domain objects and repositories
- Implements use cases (CreateQuote, AcceptQuote, etc.)
- Transaction management

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
├── InsurancePortal.QuoteService.Domain/
│   ├── Entities/
│   │   ├── Quote.cs                    (Aggregate root)
│   │   ├── ProductQuote.cs             (Abstract base)
│   │   ├── HomeQuote.cs                (Inheritance)
│   │   ├── AutoQuote.cs                (Inheritance)
│   │   └── HealthQuote.cs              (Inheritance)
│   ├── ValueObjects/
│   │   ├── HomeQuoteDetails.cs
│   │   ├── AutoQuoteDetails.cs
│   │   └── HealthQuoteDetails.cs
│   ├── Enums/
│   │   ├── QuoteStatus.cs
│   │   └── ProductType.cs
│   └── Class1.cs                       (Placeholder)
│
├── InsurancePortal.QuoteService.Application/
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   ├── CreateQuoteRequest.cs
│   │   │   ├── CreateHomeQuoteRequest.cs
│   │   │   ├── CreateAutoQuoteRequest.cs
│   │   │   └── CreateHealthQuoteRequest.cs
│   │   └── Responses/
│   │       ├── QuoteResponse.cs
│   │       ├── HomeQuoteResponse.cs
│   │       ├── AutoQuoteResponse.cs
│   │       └── HealthQuoteResponse.cs
│   ├── Services/
│   │   ├── IQuoteService.cs            (Interface)
│   │   └── QuoteService.cs             (Implementation)
│   ├── Repositories/
│   │   └── IQuoteRepository.cs         (Interface)
│   └── Class1.cs                       (Placeholder)
│
├── InsurancePortal.QuoteService.Infrastructure/
│   ├── Data/
│   │   ├── QuoteDbContext.cs           (EF DbContext)
│   │   ├── Configurations/
│   │   │   ├── QuoteConfiguration.cs
│   │   │   ├── ProductQuoteConfiguration.cs
│   │   │   ├── HomeQuoteConfiguration.cs
│   │   │   ├── AutoQuoteConfiguration.cs
│   │   │   └── HealthQuoteConfiguration.cs
│   │   └── Migrations/
│   │       └── [EF Migrations]
│   ├── Repositories/
│   │   └── QuoteRepository.cs          (Implementation)
│   ├── QUOTE_SERVICE_ARCHITECTURE.md   (This file)
│   └── Class1.cs                       (Placeholder)
│
├── InsurancePortal.QuoteService.API/
│   ├── Controllers/
│   │   └── QuotesController.cs         (REST endpoints)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json                (Configuration)
│   ├── Program.cs                      (DI & configuration)
│   └── WeatherForecast.cs              (Placeholder)
│
└── InsurancePortal.QuoteService.sln
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
- Build compilation successful

⏳ **In Progress:**
- Database migrations (working around EF Core value object serialization)

📋 **Planned:**
- Database migrations creation
- Event publishing to Azure Service Bus
- Unit tests for domain and service layer
- Integration tests for API endpoints
- Configuration management (appsettings by environment)
- Error handling middleware
- Logging and monitoring
- API versioning
- Rate limiting
- Authentication/Authorization integration with User Service

### Key Implementation Decisions

1. **Value Object Storage**
   - **Decision**: Use owned types in EF Core (flatten to parent table)
   - **Rationale**: Simpler schema, better performance, maintains DDD value object semantics
   - **Note**: Currently exploring JSON serialization alternative if needed

2. **Premium Calculation**
   - **Decision**: Implement simplified logic in domain entities
   - **Rationale**: Demonstration of product-specific logic, easily replaceable
   - **Future**: Replace with AI/ML service calls for real-world pricing

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
  "Authentication": {
	"Authority": "[Identity Provider URL]",
	"Audience": "quote-service-api"
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
