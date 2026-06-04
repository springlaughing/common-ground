# Architecture Decision Records

This directory records significant architectural and technical decisions for
CommonGround, as required by the project [constitution](../../.specify/memory/constitution.md)
§VII (*Audit-Ready DevSecOps*): *"ADRs MUST be written when significant technical
decisions are made."*

Each ADR follows a MADR-style structure: **Status · Context · Decision · Consequences ·
Alternatives considered**. ADRs are immutable once *Accepted* — a changed decision is
captured in a new ADR that *Supersedes* the old one (the old one is then marked
*Superseded by ADR-XXXX*).

**Status legend:** `Proposed` · `Accepted` · `Superseded` · `Deprecated` · `Rejected`

## Index

| ADR | Title | Status | Realizes (constitution / requirement) | Implemented by (tasks) |
|-----|-------|--------|----------------------------------------|------------------------|
| [0001](0001-use-modular-monolith.md) | Use a modular monolith for the MVP | Accepted | Principle IV — Modular Monolith | T001, T005, T006, T013 |
| [0002](0002-use-deterministic-comparison-engine.md) | Use a deterministic comparison/scoring engine | Accepted | Principles II & III — Neutral Outputs, Deterministic Engine | T022, T022b, T024, T025 |
| [0003](0003-use-private-links-and-access-codes.md) | Use private links and access codes instead of accounts | Accepted | Principles I & VI — Privacy-First, Accountless Identity | T011, T019, T023, T032, T033 |
| [0004](0004-use-postgresql-with-ef-core.md) | Use PostgreSQL with EF Core | Accepted | Storage (supports all data persistence) | T007, T008, T009, T010, T014 |
| [0005](0005-use-github-actions-for-ci-cd.md) | Use GitHub Actions for CI/CD | Accepted | Principle VII — Audit-Ready DevSecOps | T004 |

> Task IDs map to [`specs/001-questionnaire-completion/tasks.md`](../../specs/001-questionnaire-completion/tasks.md).
> Constitution principles map to [`.specify/memory/constitution.md`](../../.specify/memory/constitution.md).

## Planned / pending

Decisions made during implementation that still need an ADR (tracked, not yet written):

- **Target framework .NET 10** instead of the originally-planned .NET 9 — a deviation
  from the spec/quickstart with downstream effects (e.g. Stryker.NET incompatibility).
- **Session token mechanism** — JWT carried in an `HttpOnly; Secure; SameSite=Strict`
  cookie (`cg_session`), token read from the cookie rather than the `Authorization`
  header, plus the lazy configuration-key read that keeps the design testable. Currently
  documented only implicitly across ADR-0003, `research.md`, and the constitution.
