# CommonGround

[![CI](https://github.com/springlaughing/common-ground/actions/workflows/ci.yml/badge.svg)](https://github.com/springlaughing/common-ground/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=springlaughing_common-ground&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=springlaughing_common-ground)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=springlaughing_common-ground&metric=coverage)](https://sonarcloud.io/summary/new_code?id=springlaughing_common-ground)

A privacy-first reflection tool that helps two people understand **how they each
prefer to work** — and surfaces where they align and differ — to start a better
conversation, not to score compatibility.

**Live demo:** https://common-ground-6rsc.onrender.com
*(free tier — the first request after idle may take a few seconds to wake)*

> This is a personal hobby and portfolio project, built in the open with a
> deliberate, auditable engineering process.

## What it does

You answer a short questionnaire about your working style. You get a private
**reflection** describing your preferences in neutral, descriptive language —
never a personality label or a score. You can then share a private link so
someone else can compare their working style with yours and see where you line
up and where you'd benefit from a conversation.

The questionnaire, the reflection, and the interface are available in **English
and German**. A language switcher on every screen lets you change language at any
point — mid-questionnaire or when revisiting a saved reflection — without losing
your place or your answers.

## Principles that shape it

These are enforced, not aspirational — see the
[constitution](.specify/memory/constitution.md):

- **Privacy-first** — no accounts. Your result lives behind a private link with
  the token in the URL *fragment* (`/me#TOKEN`), so it never reaches the server
  or its logs. Tokens and access codes are stored only as hashes.
- **Neutral, non-judgmental output** — no overall "compatibility %", no
  diagnoses or personality types. Every insight is framed as an observation.
- **Deterministic & explainable** — insights come from a versioned
  questionnaire, explicit scoring rules, and predefined templates. No LLM is
  called at runtime; the same answers always produce the same result.
- **Explicit consent** at every step that shares or reuses personal data.

## Tech stack

- **Backend:** C# / .NET 10, structured as a **modular monolith** (Questionnaires,
  Responses, Comparisons, Reporting, Privacy, Audit) with module boundaries
  enforced by architecture tests.
- **Frontend:** React 19 + TypeScript + Vite. Bilingual (English / German):
  interface text ships in the frontend; questionnaire and reflection content is
  served per-language by the API (see
  [ADR-0008](docs/adr/0008-localization-translation-tables.md)).
- **Data:** PostgreSQL via EF Core.
- **Deployment:** a single container serves the SPA and API same-origin (keeps
  the session cookie first-party), on Render + Neon Postgres.

## Engineering practices

The process is part of the project:

- **Spec-driven development** with [Spec Kit](https://github.com/github/spec-kit):
  every feature flows spec → plan → tasks → issues → PR.
- A project **constitution** as the authoritative reference, plus
  **[ADRs](docs/adr/)** for significant decisions.
- **CI quality gate on every PR:** tests, SonarCloud, CodeQL, ESLint, TypeScript,
  coverage, secret scanning; dependencies kept current via Dependabot.
- **Testing:** xUnit + FluentAssertions, Testcontainers for integration tests,
  NetArchTest for module boundaries, React Testing Library for the frontend, and
  Playwright for end-to-end flows (including the bilingual journeys).

## Contributing & local setup

See **[CONTRIBUTING.md](CONTRIBUTING.md)** for the workflow and a local-dev
quickstart.

## License

Dual-licensed, on purpose:

- **Source code** — [MIT](LICENSE). Run it, learn from it, build on it.
- **Questionnaire content & product copy** — **All rights reserved**
  ([LICENSE-CONTENT.md](LICENSE-CONTENT.md)). The questions, answer options, and
  insight texts are original authored work and may not be copied or reused.
