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

**Decision**: Each `AnswerOption` carries a set of `DimensionWeight` rows (one per
dimension it contributes to, with positive or negative integer weights). After
submission, the engine accumulates weighted contributions per dimension — primary
answer at ×1.0, optional secondary answer at ×0.5. Each raw score is then normalised
against a per-dimension maximum (precomputed at seed time) to produce a 0.0–1.0
normalised score stored in `DimensionScore`. Reflection assembly filters dimensions
at a threshold (0.4), looks up pre-authored `InsightSnippet` texts, and computes a
1–5 strength indicator (ceil(normalised_score × 5)).

**Rationale**: A single answer option contributes to multiple dimensions — the
working-style questionnaire is designed this way intentionally. A single ScoringValue
per option cannot represent this. Per-dimension normalisation ensures a dimension
probed by 7 questions is not artificially stronger than one probed by 1 question.
The full pipeline is deterministic and fully testable with unit and mutation tests.
Insight snippets are human-authored strings in the database — no LLM at runtime.

**Alternatives considered**: Single ScoringValue per answer option — rejected because
it cannot represent multi-dimensional answer contributions, which is fundamental to
the questionnaire design. Score-range InsightTemplate lookup — rejected in favour of
a single pre-authored snippet per dimension, which is simpler to author and maintain.

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

**Decision**: The questionnaire has 46 questions across 10 sections, probing 76
dimensions of working style. Each question has 4 answer options (A–D); the user
selects one primary and optionally one secondary. Answer options carry
multi-dimensional weights defined in `questionary.md`. Dimensions are defined in
`dimensions.json`. User-facing insight snippets and group structure are defined in
`reflection-groups.json`. The 76 dimensions are organised into 10 named groups for
display on the personal reflection page.

Sections:
1. Collaboration setup (4 questions)
2. Staying aligned during work (3 questions)
3. Planning and delivery style (7 questions)
4. Quality, risk, and delivery expectations (5 questions)
5. Feedback and psychological safety (4 questions)
6. Autonomy and support (4 questions)
7. Meetings and live collaboration (4 questions)
8. Pressure, urgency, and motivation (4 questions)
9. Conflict and tension handling (6 questions)
10. Energy, satisfaction, and drain (5 questions)

**Rationale**: 46 questions across 10 sections is achievable in under 15 minutes
(SC-001). The multi-dimensional weight model means each answer contributes to
several dimensions simultaneously — this produces richer, more nuanced profiles than
a single-dimension-per-question design. Secondary answer support adds signal without
requiring additional questions. The 76 dimensions cover the full range of
collaboration expectations identified in the product concept.

---

## All NEEDS CLARIFICATION items resolved

No open items. Plan ready for Phase 1 design.
