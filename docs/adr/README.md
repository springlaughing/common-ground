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
| [0006](0006-target-dotnet-10.md) | Target .NET 10 (LTS) instead of .NET 9 | Accepted | Foundation — target framework / tooling | T001, T002 |
| [0007](0007-session-jwt-in-httponly-cookie.md) | Carry the session as a JWT in an HttpOnly cookie | Accepted | Principles I & VI — Privacy-First, Accountless Identity | T011, T032, T033 |
| [0008](0008-localization-translation-tables.md) | Localize content via per-locale translation tables with an English base/fallback | Accepted | Principles I, II & III — Privacy-First, Neutral Outputs, Deterministic Engine | 002: T003–T006, T021, T034, T035, T039 |

> Task IDs map to [`specs/001-questionnaire-completion/tasks.md`](../../specs/001-questionnaire-completion/tasks.md),
> except rows prefixed `002:`, which map to [`specs/002-bilingual-support/tasks.md`](../../specs/002-bilingual-support/tasks.md).
> Constitution principles map to [`.specify/memory/constitution.md`](../../.specify/memory/constitution.md).

## Maintaining this log

- Write a new ADR for each significant or spec-deviating technical decision
  (constitution §VII). Never edit an *Accepted* ADR's decision — supersede it with a new
  ADR and mark the old one *Superseded by ADR-XXXX*. (Factual reconciliation of an ADR
  with the as-built system is allowed and noted in its Status, as done for ADR-0005.)
- Add each new ADR to the index table above, with the principle it realizes and the
  tasks that implement it.
- No outstanding undocumented decisions as of 2026-06-04 — the .NET 10 and session-cookie
  decisions are now ADR-0006 and ADR-0007.
