# Research: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Date**: 2026-05-14

---

## Token Generation and Hashing

**Decision**: Use `RandomNumberGenerator.GetBytes(32)` (cryptographically secure) to
generate private result tokens and access codes. Encode as Base32 or Base64url for
URL/display safety. Hash with HMAC-SHA256 using a server-side secret for storage.
Access codes formatted as `XXXX-XXXX-XXXX` for readability.

**Rationale**: `RandomNumberGenerator` is the .NET standard for cryptographic random
generation. HMAC-SHA256 with a server secret is faster than bcrypt for token lookup
(tokens have sufficient entropy — no need for slow hashing). BCrypt/Argon2 is better
for passwords (low entropy); these tokens have 256 bits of entropy.

**Alternatives considered**: BCrypt — rejected because token entropy makes brute-force
impossible regardless; the slow hash adds latency with no security benefit here.

---

## HttpOnly Cookie Session Pattern (ASP.NET Core)

**Decision**: After validating a private result token, issue a short-lived JWT inside
an `HttpOnly; Secure; SameSite=Strict` cookie using ASP.NET Core cookie authentication
middleware. The JWT payload contains only the ResponseSet ID (no raw token). Expiry: 30
days. If the cookie expires, the user re-validates using their saved fragment URL.

**Rationale**: ASP.NET Core's `AddCookie` authentication handles cookie issuance,
validation, renewal, and revocation cleanly. SameSite=Strict prevents CSRF for
same-origin requests. The cookie never exposes the token after first validation.

**Alternatives considered**: Server-side session store — rejected for MVP because it
requires Redis or database session table; stateless JWT in cookie is simpler and
sufficient.

---

## Deterministic Scoring Engine

**Decision**: Each `AnswerOption` has a `ScoringValue` (integer). After submission, the
engine groups answers by dimension, sums scoring values per dimension, and looks up the
matching `InsightTemplate` for each dimension based on score range. Personal reflection
is the set of matched insight texts. No calculation beyond summing and range lookup.

**Rationale**: Simple, testable, reproducible. The scoring logic can be fully covered
by unit and mutation tests. Insight templates are human-reviewed strings stored in the
database — not generated at runtime.

**Alternatives considered**: Weighted scoring with normalisation — rejected as
unnecessary complexity for MVP. Can be introduced with a new questionnaire version
later.

---

## EF Core Data Seeding

**Decision**: Use EF Core `HasData()` in `OnModelCreating` to seed the initial
questionnaire version, questions, answer options, scoring rules, and insight templates.
Seed data is part of the migration — reproducible and version-controlled.

**Rationale**: `HasData()` seeds are tied to migrations, so the questionnaire content
is always in sync with the schema. Easy to test in integration tests via Testcontainers.

**Alternatives considered**: SQL seed scripts — rejected because they require separate
management outside EF Core migrations.

---

## React Step-by-Step Questionnaire

**Decision**: Use a simple `currentStepIndex` state integer in React. Steps array
contains: `[ConsentStep, ...QuestionSteps, CredentialsDisplayStep]`. Each step
component receives `onNext` / `onBack` callbacks. No external state machine library.

**Rationale**: The questionnaire has a linear flow with no branching. A simple index +
array is sufficient and keeps the component tree shallow. A state machine library
(XState) would be over-engineering for a linear flow.

**Alternatives considered**: React Router step-per-route — rejected because it puts
the token in the URL and adds browser history entries for each question, which is not
desired. XState — rejected as unnecessary for a linear flow.

---

## NetArchTest Module Boundary Enforcement

**Decision**: `CommonGround.ArchitectureTests` uses NetArchTest to assert that no
module project directly references another module project. All cross-module
communication goes through interfaces defined in `CommonGround.SharedKernel`.
`CommonGround.Api` is the only project allowed to reference all modules (for DI wiring).

**Rationale**: Enforces the modular monolith principle from the constitution. Violations
are caught at build time in CI, not in code review.

**Example rule**:
```csharp
Types.InAssembly(typeof(ResponsesModule).Assembly)
    .Should()
    .NotHaveDependencyOn("CommonGround.Modules.Questionnaires")
    .GetResult().IsSuccessful
```

---

## Questionnaire Content (MVP)

**Decision**: Design a working-style questionnaire with 12 questions across 6
dimensions. Each question has 4 answer options scored 1–4. Dimensions:

1. **FeedbackRhythm** — how often and in what form feedback is preferred
2. **DecisionMaking** — autonomous vs. consensus-driven decisions
3. **CommunicationStyle** — written vs. verbal, async vs. sync
4. **DeadlineApproach** — flexible vs. strict, buffer vs. just-in-time
5. **ConflictHandling** — direct vs. indirect, immediate vs. delayed
6. **CollaborationDepth** — independent work vs. close collaboration

2 questions per dimension. Insight templates cover low (1–4), mid (5–6), and high
(7–8) score ranges per dimension for personal reflection.

**Rationale**: 12 questions is achievable in under 15 minutes (SC-001). 6 dimensions
covers the key working-style areas identified in the product concept. 3 insight bands
per dimension = 18 personal reflection insight templates for the MVP.

---

## All NEEDS CLARIFICATION items resolved

No open items. Plan ready for Phase 1 design.
