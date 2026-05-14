# ADR 0004: Use PostgreSQL with EF Core

## Status

Accepted

## Context

CommonGround requires a relational database. The domain model is clearly relational:
`ResponseSet` references `QuestionnaireVersion`, `ComparisonSession` references multiple
`ResponseSet`s, `AuditEvent` references sessions and participants. Referential integrity
and transactional consistency are important — for example, attaching a response to a
comparison and recording the consent audit event must be atomic.

The backend is C# / .NET. The database must support concurrent access (two participants
can submit responses simultaneously, triggering comparison generation), run locally in
development without manual setup, and integrate cleanly with the integration test strategy.

## Decision

Use **PostgreSQL 16** as the database, **EF Core** as the ORM with code-first migrations,
and **Docker Compose** for local development.

**Local development**: `docker-compose.yml` at the repo root defines a PostgreSQL 16
container. Developers run `docker compose up -d` to start the database. No manual
PostgreSQL installation is required. The connection string lives in
`appsettings.Development.json` (not committed — added to `.gitignore`).

**ORM**: EF Core with Npgsql provider. Domain entities are defined as C# classes.
Migrations are generated via EF Core CLI and committed to the repository. Migrations
run on application startup in development; in production they are applied as a
controlled step in the deployment pipeline.

**Integration tests**: Testcontainers.NET with the PostgreSQL module spins up an
isolated PostgreSQL container per test run. Integration tests never touch the dev
database. This satisfies the constitution requirement for real database integration
tests with no mocking.

## Consequences

### Positive

- Docker Compose config is version-controlled — reproducible local setup with no
  manual steps.
- Npgsql + EF Core is a mature, well-supported combination for .NET.
- PostgreSQL supports UUID, JSONB, and array types natively — useful if questionnaire
  definitions or insight templates are partially stored as JSON in future.
- Testcontainers.NET gives integration tests a real PostgreSQL instance, not a mock
  or in-memory substitute.
- PostgreSQL is available on all major hosting platforms with free tiers (Railway,
  Supabase, Neon, Render, Azure Database for PostgreSQL).

### Negative

- Developers need Docker Desktop installed locally.
- EF Core migrations must be managed carefully — migrations are immutable once
  applied to a shared environment.

## Alternatives considered

### SQLite

SQLite requires no server and works well for unit testing with EF Core in-memory.
Rejected because it has limited concurrent write support, which is not suitable for
production where multiple participants submit responses simultaneously. SQLite may
still be used in unit tests where no real database behaviour is needed.

### MySQL

MySQL is a capable relational database but offers no advantage over PostgreSQL for
this stack. PostgreSQL has better .NET ecosystem support (Npgsql), richer data types,
and stronger ACID guarantees. Rejected in favour of PostgreSQL.

### SQL Server

SQL Server has excellent EF Core support and is Microsoft's native choice. Rejected
because it requires a paid license for production use and adds operational cost
without benefit for an MVP. The free Developer edition is not suitable for deployed
environments.

## Future options

- Connection string management via environment variables or a secrets manager in
  production (e.g. Azure Key Vault, Railway secrets).
- Read replicas if reporting queries become expensive.
- EF Core compiled models for startup performance if the schema grows large.
