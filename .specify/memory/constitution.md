# CommonGround Constitution

## Core Principles

### I. Privacy-First

All product and technical decisions MUST minimize personal data exposure.

- Raw answers MUST NOT be visible to other participants or appear in logs
- Private result tokens and access codes MUST be stored as hashes only, never plain text
- Invite links MUST be time-limited and single-use
- Audit logs MUST record lifecycle events but MUST NOT contain raw answers, tokens, access codes, or full result text
- Users MUST be able to delete their response at any time; deletion MUST disable all associated access
- The app MUST NOT automatically identify returning users — identification is always user-initiated

Rationale: CommonGround handles interpersonal working-style data shared in sensitive professional contexts. Privacy is the foundation of trust, not a feature added on top.

### II. Neutral and Non-Judgmental Outputs

All user-facing output MUST use neutral, descriptive language.

- Overall compatibility scores are PROHIBITED (e.g. "87% compatible", "good fit", "hire/reject")
- Psychological diagnoses, emotional intelligence scores, and personality labels are PROHIBITED
- Per-dimension alignment indicators are ALLOWED when framed descriptively, not as verdicts (e.g. a visual bar showing alignment on "feedback rhythm", or "you align on 5 of 8 dimensions")
- Every insight MUST be framed as an observation (e.g. "You both prefer explicit expectations", not "You are compatible")
- Conversation prompts MUST be open-ended and non-prescriptive

Rationale: The app is a reflection tool, not a decision-making engine. A single overall score reduces nuanced working-style differences to a number that invites misuse, especially in hiring contexts. Per-dimension indicators remain factual and useful without creating a verdict.

### III. Deterministic and Explainable Engine

The comparison and reflection engine MUST be deterministic.

- All insights MUST be derived from versioned questionnaire definitions, explicit scoring rules, and predefined insight templates
- No LLM MUST be called at runtime to interpret individual users or generate psychological conclusions in the MVP
- Every insight MUST be traceable to specific questionnaire dimensions or questions
- Insight templates MAY be drafted with AI assistance during development but MUST be human-reviewed, stored, versioned, and served deterministically
- The same questionnaire version and the same answers MUST always produce the same output

Rationale: Explainability and reproducibility are essential for user trust and safe operation in professional contexts. Black-box interpretation is inconsistent with the privacy-first and non-judgmental principles, and makes testing impossible.

### IV. Modular Monolith

The backend MUST be structured as a modular monolith.

- Modules: Questionnaires, Responses, Comparisons, Reporting, Privacy, Audit, Notifications
- Module boundaries MUST be enforced by architecture tests — direct cross-module access is prohibited
- Each module MUST expose a clearly defined public interface; inter-module calls go through interfaces only
- The data model MUST support group comparisons from day one to avoid future migrations
- Future extraction of modules MUST remain possible without data model changes

Rationale: The MVP does not require independent service deployment or separate scaling. Microservices at this stage add distributed-system complexity without benefit. Enforced boundaries preserve the option to extract modules later.

### V. Explicit Consent at Every Step

Explicit consent MUST be obtained before any action that involves sharing or reusing personal data.

- An invited participant MUST explicitly consent before their answers are compared with the inviter's
- Response reuse MUST require explicit confirmation for each specific comparison
- The reuse confirmation prompt MUST name the specific person the response will be compared against
- Audit events MUST record that consent was given
- Deletion behavior MUST be clearly explained before the user confirms

Rationale: Trust in the product depends on users understanding exactly what they are agreeing to. Implicit or automatic consent is inconsistent with the privacy-first principle.

### VI. Accountless, Credential-Minimal Identity

The MVP MUST operate without user accounts.

- Access to personal results MUST use private result links with the token in the URL fragment: `/me#TOKEN`
- The token MUST be delivered via fragment (not URL path) so it is never transmitted to the server in HTTP requests and does not appear in server or proxy logs
- On first visit, the React frontend reads the fragment and POSTs the token to `/api/session/start`; the server validates the hash and issues a `HttpOnly; Secure; SameSite=Strict` session cookie
- Subsequent requests use the cookie — the token is not re-transmitted after the first validation
- Response reuse MUST use access codes only (`K7Q9-MP2D-W4T8` format)
- Private result links and access codes are intentionally non-interchangeable and MUST serve only their designated purpose
- The backend MUST store only hashes of tokens and access codes
- Access code reuse MUST be user-initiated — the app MUST NOT automatically attach existing responses to comparisons
- If a user loses both their private result link and access code, the MVP provides no recovery path — this is by design

Rationale: Accounts introduce authentication, recovery, and PII complexity incompatible with the privacy-first MVP. Private links and access codes provide lightweight, transparent access control without collecting email or identity. The fragment + cookie approach avoids token exposure in server logs while keeping the URL bookmarkable.

### VII. Audit-Ready DevSecOps

Every important claim about the software MUST have verifiable evidence.

- CI MUST produce test reports, coverage reports, and a quality gate result as required checks on every PR
- No new critical vulnerabilities or secrets MUST be introduced in any merge
- Mutation testing MUST be run on the comparison and scoring engine — it is the critical business logic
- A task is not complete until acceptance criteria are covered by tests and CI passes
- Each PR MUST be linked to a requirement, spec, or issue
- ADRs MUST be written when significant technical decisions are made

Rationale: CommonGround handles consent, sensitive personal data, and access-controlled results. "We have CI" is not the same as "we have auditability." The evidence chain answers: was this reviewed, did tests pass, was quality enforced, was it built and deployed correctly?

## Security and Privacy Rules

**Credentials and tokens**:
- Private result tokens and access codes MUST use sufficient entropy to resist guessing attacks
- The backend MUST store only hashes of tokens and access codes — never plain values
- The backend MUST implement rate limiting on access code entry to resist brute-force attacks
- Session cookies MUST be `HttpOnly; Secure; SameSite=Strict` with a defined expiry

**API security baseline**:
- All traffic MUST use HTTPS — HTTP requests MUST be redirected or rejected
- All API responses MUST include security headers: `Strict-Transport-Security`, `Content-Security-Policy`, `Referrer-Policy: no-referrer`, `X-Content-Type-Options: nosniff`
- Input MUST be validated at all API boundaries — never trust client-supplied data
- Error responses MUST NOT leak internal details, stack traces, or token values
- Rate limiting MUST be applied to all public-facing endpoints, not only access code entry
- CORS MUST be explicitly configured — wildcard origins are prohibited

**Audit logs**:
- Audit logs MUST be append-only
- Audit logs MUST record: consent events, reuse events, comparison generation, invite expiry, response deletion, and access denial
- Audit logs MUST NOT record: raw answers, tokens, access codes, full result text, or sensitive personal content

**Invite links**:
- Invite links MUST expire after a fixed time limit or upon first successful use, whichever comes first
- A grace period MUST be provided if an invite expires while the invitee is mid-flow

## Development Standards

**Stack**: C# / .NET backend, React + TypeScript frontend.

**Backend testing**:
- TDD is MANDATORY: tests MUST be written before implementation
- xUnit, FluentAssertions, Moq for unit tests
- WireMock.Net for external HTTP dependency simulation
- Testcontainers.NET for integration tests
- NetArchTest or ArchUnitNET for architecture tests — module boundary enforcement is non-negotiable
- Stryker.NET mutation testing MUST run on the comparison and scoring engine
- Smoke tests MUST run after every deployment

**Frontend standards**:
- TypeScript strict mode is MANDATORY — no `any`, no implicit types
- ESLint with a consistent ruleset enforced in CI
- React Testing Library for component tests
- Playwright for end-to-end tests covering critical user flows (consent, comparison join, response reuse, deletion)
- Frontend coverage and linting MUST be included in the CI quality gate

**Accessibility**:
- All UI MUST meet WCAG 2.1 AA as a minimum target
- Consent screens and result pages MUST be keyboard-navigable and screen-reader compatible

**Consent UX (non-negotiable)**:
- Consent screens MUST present accept and decline options with equal visual weight — no large prominent accept button paired with a small or hidden decline
- Consent language MUST be specific: exactly what will be shared, with whom, and for what purpose
- Declining MUST always be available and MUST NOT guilt-trip the user
- The response reuse confirmation MUST name the specific person the response will be compared against
- Deletion confirmation MUST list all consequences before the user confirms, not after
- No countdown timers, urgency language, or scarcity framing on consent screens
- Double negatives in consent language are PROHIBITED

**Quality gates (enforced on every PR)**:
- All tests pass (backend and frontend)
- No new critical bugs or vulnerabilities
- No secrets detected
- ESLint passes with no errors
- TypeScript compiles with no errors
- Coverage on new code ≥ 80% (initial target 70%, ramp to 80%)
- Mutation score on domain/comparison layer ≥ 60% (target 75%)
- SonarCloud quality gate passes

**Data model**:
- MUST support group comparisons from day one
- Questionnaire versions are immutable once responses exist — a new version requires new responses

**Definition of done**: a task is complete only when acceptance criteria are covered by tests, CI passes, and documentation is updated if behavior changed.

## Governance

This constitution supersedes all other development practices and guidelines for CommonGround.

- All implementation decisions MUST be validated against these principles before proceeding
- Any amendment MUST be documented with: the changed principle, the rationale, and a migration plan for affected code or data
- Version increments follow semantic versioning:
  - MAJOR: backward-incompatible removal or redefinition of a principle
  - MINOR: new principle or section added, or materially expanded guidance
  - PATCH: clarifications, wording fixes, non-semantic refinements
- Compliance review is expected at every PR
- The constitution is the authoritative reference for resolving design disputes

**Version**: 1.1.0 | **Ratified**: 2026-05-14 | **Last Amended**: 2026-05-14
