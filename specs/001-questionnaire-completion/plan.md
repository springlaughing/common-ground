# Implementation Plan: Questionnaire Completion

**Branch**: `001-questionnaire-completion` | **Date**: 2026-05-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-questionnaire-completion/spec.md`

## Summary

A new user opens the app, acknowledges a privacy and consent explanation, fills in the
questionnaire one question at a time, and receives a personal reflection page with
neutral working-style insights. They also receive a private result link (for returning
to their reflection) and an access code (for reusing their response in future
comparisons). The backend uses a deterministic scoring engine — versioned questionnaire
definitions, explicit scoring rules, and predefined insight templates — with no LLM at
runtime. This is the foundational feature; all other MVP features depend on it.

---

## Technical Context

**Language/Version**: C# 14 / .NET 10 (backend), TypeScript 5 / React 18 (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core 10, EF Core 10 + Npgsql, xUnit, FluentAssertions, Moq,
  Testcontainers.NET, NetArchTest, Stryker.NET, WireMock.Net
- Frontend: React 18, TypeScript 5, Vite, React Testing Library, Playwright, ESLint

**Storage**: PostgreSQL 16 — Docker Compose locally, Testcontainers.NET for integration tests

**Testing**: xUnit + FluentAssertions + Moq (unit), Testcontainers.NET (integration),
NetArchTest (architecture), React Testing Library (component), Playwright (E2E),
Stryker.NET on scoring engine (mutation)

**Target Platform**: Web service on Linux container (backend), modern browser (frontend)

**Project Type**: Web application — ASP.NET Core REST API + React SPA

**Performance Goals**: Full questionnaire completion flow under 15 minutes (SC-001);
page transitions under 300ms perceived

**Constraints**: No user accounts; all tokens and access codes hashed; WCAG 2.1 AA;
HTTPS only; cookies HttpOnly + Secure + SameSite=Strict

**Scale/Scope**: MVP portfolio project — small concurrent user base, single active
questionnaire version

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. Privacy-First | Raw answers not in logs or reports; tokens/codes hashed; audit events contain no raw answers (FR-014) | ✅ Pass |
| II. Neutral Outputs | Personal reflection uses insight templates only; no overall score; insights framed as observations (FR-007, SC-004) | ✅ Pass |
| III. Deterministic Engine | Scoring engine uses versioned definitions + scoring rules + insight templates; no LLM at runtime; same inputs = same output | ✅ Pass |
| IV. Modular Monolith | Backend split into Questionnaires, Responses, Reporting, Privacy, Audit, Comparisons modules; architecture tests enforce boundaries; ComparisonSession/ComparisonParticipant tables created now for group support | ✅ Pass |
| V. Explicit Consent | Consent explanation shown and acknowledged before questionnaire (FR-001, FR-002); audit event logged | ✅ Pass |
| VI. Accountless Identity | Private result link via fragment token; access code for reuse only; both hashed server-side; HttpOnly cookie session after first validation | ✅ Pass |
| VII. DevSecOps | CI workflows, Dependabot, SonarCloud, branch protection set up in foundational phase before feature code | ✅ Pass |

No violations. No complexity justification required.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-questionnaire-completion/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── api.md           ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── CommonGround.Api/                    # ASP.NET Core host, DI wiring, middleware
│   ├── CommonGround.Modules.Questionnaires/ # QuestionnaireVersion, Question, AnswerOption, ScoringRule, InsightTemplate
│   ├── CommonGround.Modules.Responses/      # ResponseSet, Answer
│   ├── CommonGround.Modules.Reporting/      # Deterministic scoring engine, reflection generation
│   ├── CommonGround.Modules.Privacy/        # Token/code generation, hashing, session management
│   ├── CommonGround.Modules.Audit/          # AuditEvent logging
│   ├── CommonGround.Modules.Comparisons/    # ComparisonSession, ComparisonParticipant (stub — tables only)
│   └── CommonGround.SharedKernel/           # Shared primitives, module interfaces, domain exceptions
├── tests/
│   ├── CommonGround.UnitTests/              # Domain logic, scoring engine, token generation
│   ├── CommonGround.IntegrationTests/       # API endpoints, EF Core, PostgreSQL via Testcontainers
│   └── CommonGround.ArchitectureTests/      # NetArchTest module boundary enforcement
├── CommonGround.sln
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── NuGet.config
└── .editorconfig

frontend/
├── src/
│   ├── pages/
│   │   ├── QuestionnairePage/               # Consent step + question steps + credentials display
│   │   └── ReflectionPage/                  # Personal reflection view (loaded via /me#TOKEN)
│   ├── components/
│   │   ├── ConsentStep/                     # Privacy explanation + acknowledgment
│   │   ├── QuestionStep/                    # Single question display + answer selection
│   │   ├── ProgressIndicator/               # "3 of 12" progress bar
│   │   └── CredentialsDisplay/              # Private result link + access code with explanations
│   ├── services/
│   │   └── questionnaireApi.ts              # API client
│   └── types/
│       └── api.ts                           # Shared API types
├── tests/
│   ├── components/                          # React Testing Library component tests
│   └── e2e/                                 # Playwright E2E tests
├── package.json
├── tsconfig.json
├── vite.config.ts
└── .eslintrc.json

.github/
├── workflows/
│   ├── ci.yml
│   ├── security.yml
│   └── release.yml
└── dependabot.yml
```

**Structure Decision**: Web application layout with separate `backend/` and `frontend/`
directories. Backend is a modular monolith — each module is a separate .NET project to
enable NetArchTest boundary enforcement. Frontend is a React SPA built with Vite.

---

## Complexity Tracking

No constitution violations to justify.
