
# InsurancePortal.QuoteService

A modular .NET backend service for insurance quote processing, built with a layered architecture and a focus on maintainability, separation of concerns, and testability.

## Overview

InsurancePortal.QuoteService is part of an insurance portal solution designed to handle insurance quote-related business operations.

The project demonstrates how to structure a backend service using distinct architectural layers, keeping business logic independent from API and infrastructure concerns.

## Architecture

The solution is organized into the following projects:

- **API** — Entry point for client requests and application endpoints.
- **Application** — Application workflows and business use cases.
- **Domain** — Core domain models and business rules.
- **Infrastructure** — Infrastructure and external dependency implementations.
- **Tests** — Automated tests for verifying service behavior.

This separation helps improve code organization, maintainability, and the ability to evolve individual components independently.

## Solution Structure

```text
InsurancePortal.QuoteService/
├── InsurancePortal.QuoteService.API/
├── InsurancePortal.QuoteService.Application/
├── InsurancePortal.QuoteService.Domain/
├── InsurancePortal.QuoteService.Infrastructure/
├── InsurancePortal.QuoteService.Tests/
└── InsurancePortal.QuoteService.slnx
```

## Getting Started

### Prerequisites

- .NET SDK compatible with the solution
- Visual Studio 2022 or Visual Studio Code
- Git

### Clone the Repository

```bash
git clone https://github.com/krutalmodi/InsurancePortal.QuoteService.git
cd InsurancePortal.QuoteService
```

### Restore Dependencies

```bash
dotnet restore InsurancePortal.QuoteService.slnx
```

### Build the Solution

```bash
dotnet build InsurancePortal.QuoteService.slnx
```

### Run Tests

```bash
dotnet test InsurancePortal.QuoteService.slnx
```

### Run the API

Open the solution in Visual Studio and set
`InsurancePortal.QuoteService.API` as the startup project.

Alternatively, run the API project from the command line:

```bash
dotnet run --project InsurancePortal.QuoteService.API
```

The API address and available endpoints depend on the application configuration.

## Design Principles

This project follows architectural principles intended to support:

- Separation of concerns
- Maintainable and modular code
- Clear boundaries between business logic and infrastructure
- Testable application components
- Extensible insurance quote business workflows

## Purpose

This project is part of an ongoing effort to build practical, domain-oriented backend services using the Microsoft .NET ecosystem.

It serves as a learning and development project for applying software architecture principles to insurance technology solutions.

## Author

**Krutal Modi**

GitHub: [@krutalmodi](https://github.com/krutalmodi)

## License

Add your preferred open-source license here.
