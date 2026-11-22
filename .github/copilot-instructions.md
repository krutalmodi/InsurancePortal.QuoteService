# Copilot Instructions

## Project Guidelines
- Any updates to the solution, project, or code must be reflected in InsurancePortal.QuoteService.Infrastructure\QuoteService-Architecture.md, because it is the key markdown file maintained for technical and logical implementation details.
- Use the same code structure and patterns as the QuoteService project when building other services (for example, LocationService). Repositories go in Infrastructure/Repositories, and startup follows QuoteService Program.cs.
- CQRS is mandatory for all new services built from the boilerplate template; Quote and Location services will be migrated to CQRS later.
- Lookup tables use int IDs (already implemented in LocationService).