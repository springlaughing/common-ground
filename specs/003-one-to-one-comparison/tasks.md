---
description: "Task list for 003-one-to-one-comparison"
---

# Tasks: One-to-One Comparison (Core Happy-Path)

**Input**: Design documents from `specs/003-one-to-one-comparison/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/api.md](contracts/api.md)

**Tests**: Included — the constitution mandates TDD (tests before implementation) and mutation-grade coverage on the comparison engine.

**Organization**: Grouped by user story (US1–US4) for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- File paths are repository-relative.

---

## Phase 1: Setup (Shared Infrastructure)

- [ ] T001 Register the Comparisons module services and wire it into the API host in `backend/src/CommonGround.Modules.Comparisons/ComparisonsModuleExtensions.cs` and `backend/src/CommonGround.Api/Program.cs`
- [ ] T002 [P] Add an `/invite` path route (alongside `/` and `/me`) with a placeholder page in `frontend/src/main.tsx`
- [ ] T003 [P] Add the empty comparison/invite/consent groups to the `Messages` interface so both catalogs must implement them, in `frontend/src/i18n/messages.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 Create the `Invite` entity (TokenHash, ComparisonSessionId, InviterResponseSetId, InviterLabel, Status, ExpiresAt, CreatedAt) in `backend/src/CommonGround.Modules.Comparisons/Entities/Invite.cs`
- [ ] T005 [P] Add `DisplayLabel` to `ComparisonParticipant` in `backend/src/CommonGround.Modules.Comparisons/Entities/ComparisonParticipant.cs`
- [ ] T006 [P] EF configurations (unique `Invite.TokenHash`; indexes on `TokenHash` and `ComparisonParticipant.ResponseSetId`; unique `(ComparisonSessionId, ResponseSetId)`) in `backend/src/CommonGround.Api/Persistence/Configurations/`
- [ ] T007 Hand-author the `AddComparisons` migration (Invite + ComparisonSession + ComparisonParticipant tables) in `backend/src/CommonGround.Api/Persistence/Migrations/` (`dotnet ef` is blocked on this machine — author by hand, mirroring prior migrations)
- [ ] T008 [P] Define `IComparisonService` + DTOs (`ComparisonSummaryDto`, `ComparisonReportDto`, `ComparisonInsightDto`) in `backend/src/CommonGround.SharedKernel/Interfaces/IComparisonService.cs`
- [ ] T009 [P] Expose per-dimension localized title+text for reuse by the comparison assembler (extend `IReportingService` / a reader) in `backend/src/CommonGround.SharedKernel/Interfaces/IReportingService.cs` and `backend/src/CommonGround.Modules.Reporting/`
- [ ] T010 [P] Architecture test: `Comparisons` reaches `Reporting`/`Responses` only via SharedKernel interfaces and holds no raw-answer types, in `backend/tests/CommonGround.ArchitectureTests/`

**Checkpoint**: schema, module wiring, and interfaces ready.

---

## Phase 3: User Story 1 - Inviter creates an invite (Priority: P1) 🎯 MVP

**Goal**: A person with a completed reflection creates a single-use, time-limited invite link (with their self-label); their `/me` shows the comparison as pending.

**Independent Test**: With a session, `POST /api/comparisons` returns an invite token + pending status; the link does not expose the inviter's results; a second call yields a distinct invite.

### Tests (write first, must fail)

- [ ] T011 [P] [US1] Unit test: invite token generate/hash/expiry-window in `backend/tests/CommonGround.UnitTests/Comparisons/InviteTokenServiceTests.cs`
- [ ] T012 [P] [US1] Integration test: `POST /api/comparisons` creates session+initiator+invite, returns token, pending; requires session; rejects missing/too-long label; audit `comparison_invite_created`, in `backend/tests/CommonGround.IntegrationTests/CreateInviteTests.cs`

### Implementation

- [ ] T013 [US1] `InviteTokenService` (generate/hash via `Privacy.TokenService`, compute expiry) in `backend/src/CommonGround.Modules.Comparisons/Services/InviteTokenService.cs`
- [ ] T014 [US1] `ComparisonService.CreateInvite` (ComparisonSession[Pending] + Initiator participant w/ label + Invite[Active]) in `backend/src/CommonGround.Modules.Comparisons/Services/ComparisonService.cs`
- [ ] T015 [US1] `ComparisonsController` `POST /api/comparisons` (session-auth, rate-limited, validates label, audit) in `backend/src/CommonGround.Api/Controllers/ComparisonsController.cs`
- [ ] T016 [P] [US1] `createInvite` in `frontend/src/services/comparisonApi.ts`
- [ ] T017 [US1] `InviteCreate` component (label input → shareable `/invite#token` link) mounted on the reflection/`/me` page, in `frontend/src/components/InviteCreate/`
- [ ] T018 [P] [US1] Component test: `InviteCreate` renders, posts, surfaces the link, in `frontend/tests/components/InviteCreate.test.tsx`
- [ ] T019 [P] [US1] Invite-create chrome strings (en+de) in `frontend/src/i18n/messages.en.ts` and `messages.de.ts`

**Checkpoint**: an inviter can mint and copy an invite link.

---

## Phase 4: User Story 2 - Invitee consents and joins (Priority: P2)

**Goal**: The invitee opens the link, sees the inviter's label + a §V-compliant consent screen, explicitly consents, completes the questionnaire, and receives their own private link, access code, and reflection.

**Independent Test**: `POST /api/invite/validate` returns the inviter label without consuming; declining creates nothing; consenting + completing creates the invitee response, consumes the invite (single-use), and returns the invitee's own credentials.

### Tests (write first, must fail)

- [ ] T020 [P] [US2] Integration test: validate (no consume); join with consent creates invitee response+participant+label, consumes invite, returns own link/code, starts session; rejects no-consent, used/expired (outside grace), version mismatch; audit `comparison_invite_opened`/`comparison_joined`, in `backend/tests/CommonGround.IntegrationTests/JoinInviteTests.cs`
- [ ] T021 [P] [US2] Unit test: consent-required gate, single-use `Active→Used` transition, grace window in `backend/tests/CommonGround.UnitTests/Comparisons/InviteJoinRulesTests.cs`

### Implementation

- [ ] T022 [US2] `ComparisonService.ValidateInvite` (no consume; audit `comparison_invite_opened`) in `backend/src/CommonGround.Modules.Comparisons/Services/ComparisonService.cs`
- [ ] T023 [US2] `ComparisonService.Join` (consent gate; create invitee `ResponseSet` via Responses; Invitee participant + label; consume invite; start session; audit `comparison_joined`) in the same service
- [ ] T024 [US2] `ComparisonsController` `POST /api/invite/validate` + `POST /api/invite/join` (rate-limited, neutral errors) in `backend/src/CommonGround.Api/Controllers/ComparisonsController.cs`
- [ ] T025 [P] [US2] `validateInvite` + `joinInvite` in `frontend/src/services/comparisonApi.ts`
- [ ] T026 [US2] `InvitePage` (read fragment token → validate → consent → questionnaire → own credentials) in `frontend/src/pages/InvitePage/InvitePage.tsx`
- [ ] T027 [US2] Invitee comparison-consent variant (equal-weight accept/decline, specific copy, self-label field disclosed) in `frontend/src/components/ConsentStep/` (en+de)
- [ ] T028 [P] [US2] Component test: `InvitePage` decline creates nothing / accept proceeds; consent copy passes §V (no double negatives, equal weight), in `frontend/tests/components/InvitePage.test.tsx`
- [ ] T029 [P] [US2] Invitee consent + invite chrome strings (en+de) reviewed against §V in `frontend/src/i18n/messages.en.ts` and `messages.de.ts`

**Checkpoint**: an invitee can consent, complete, and get their own results; the second response now exists.

---

## Phase 5: User Story 3 - Comparison generates automatically (Priority: P3)

**Goal**: Once both responses exist on the same version, a deterministic pair comparison is produced with no user action.

**Independent Test**: Given two completed responses on one version, the session becomes `Complete` and regenerating yields identical output; version mismatch is rejected.

### Tests (write first, must fail)

- [ ] T030 [P] [US3] Unit test (engine, exhaustive — compensates the blocked Stryker run): per-dimension classification (similar/difference/omitted), threshold/gap rules, **symmetry** (A↔B equals B↔A per viewpoint), **determinism** (repeat identical), in `backend/tests/CommonGround.UnitTests/Comparisons/ComparisonEngineTests.cs`
- [ ] T031 [P] [US3] Integration test: two responses same version → `Complete` + `comparison_generated` audit (once); cross-version guarded, in `backend/tests/CommonGround.IntegrationTests/ComparisonGenerationTests.cs`

### Implementation

- [ ] T032 [US3] `ComparisonEngine` — pure function over two dimension-score sets → classified per-dimension result, in `backend/src/CommonGround.Modules.Comparisons/Services/ComparisonEngine.cs`
- [ ] T033 [US3] Generation in `ComparisonService` (mark `Complete` when both responses present; same-version assertion; audit `comparison_generated` once) in `backend/src/CommonGround.Modules.Comparisons/Services/ComparisonService.cs`

**Checkpoint**: a completed pair yields a deterministic comparison.

---

## Phase 6: User Story 4 - Both participants view the report (Priority: P1)

**Goal**: Each participant opens their own `/me`, sees all their comparisons listed, and opens a report — differ-then-similar, second person for both, neutral, bilingual, no raw answers.

**Independent Test**: `GET /api/me/comparisons` lists the caller's comparisons; `GET /api/me/comparisons/{id}?locale=` returns the per-viewer report with the right ordering/language and no score/raw answers; a non-participant gets `403` + audit.

### Tests (write first, must fail)

- [ ] T034 [P] [US4] Integration test: list endpoint; report endpoint differ-then-similar + per-viewer + EN/DE + no score/raw answers; `403` non-participant + `access_denied`; pending/unavailable markers, in `backend/tests/CommonGround.IntegrationTests/ViewComparisonTests.cs`
- [ ] T035 [P] [US4] Unit test: `ComparisonAssembler` combines engine classification + Reporting localized text, ordering, English fallback, in `backend/tests/CommonGround.UnitTests/Comparisons/ComparisonAssemblerTests.cs`

### Implementation

- [ ] T036 [US4] `ComparisonAssembler` (engine classification + Reporting localized title/text → `ComparisonReportDto`; differ-then-similar; optional neutral "N of M" count) in `backend/src/CommonGround.Modules.Comparisons/Services/ComparisonAssembler.cs`
- [ ] T037 [US4] `MeController` `GET /api/me/comparisons` + `GET /api/me/comparisons/{id}?locale=` (session-auth, per-viewer perspective, audit `access_denied`) in `backend/src/CommonGround.Api/Controllers/MeController.cs`
- [ ] T038 [P] [US4] `listComparisons` + `getComparison` (locale-aware) in `frontend/src/services/comparisonApi.ts`
- [ ] T039 [US4] `/me` hub: list comparisons (pending/ready) + open, in `frontend/src/pages/ReflectionPage/MeReflection.tsx` and `frontend/src/components/ComparisonList/`
- [ ] T040 [US4] Wire `ComparisonPage` to the API (replace demo data), per-viewer, bilingual, switchable at view time, in `frontend/src/pages/ComparisonPage/ComparisonPage.tsx`
- [ ] T041 [P] [US4] Component tests: comparison list; `ComparisonPage` renders differ-then-similar, no score, language switch, in `frontend/tests/components/`
- [ ] T042 [P] [US4] Comparison report chrome strings (en+de) in `frontend/src/i18n/messages.en.ts` and `messages.de.ts`

**Checkpoint**: both participants view their report(s); `/me` lists multiple comparisons.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T043 [P] E2E (Playwright): create invite → invitee consent+complete → both view report → second comparison listed, in `frontend/tests/e2e/comparison.spec.ts`
- [ ] T044 [P] Accessibility pass: invite/consent/comparison pages WCAG 2.1 AA (keyboard + screen reader, switcher, German wrap) across `frontend/src/`
- [ ] T045 [P] ADR: deterministic comparison engine + compute-on-read, in `docs/adr/`
- [ ] T046 [P] ADR: invite token model (fragment, single-use) + per-participant display labels (the consent/no-name revisit), in `docs/adr/`
- [ ] T047 [P] Update README/CONTRIBUTING for comparison + run quickstart validation against SC-001…SC-010, in `README.md`, `CONTRIBUTING.md`, `specs/003-one-to-one-comparison/quickstart.md`
- [ ] T048 Confirm the CI quality gate (coverage ≥ 80% new code, SonarCloud, CodeQL) is green; record the engine mutation-testing gap against issue #18

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (P1)** → no deps.
- **Foundational (P2)** → after Setup; **blocks all stories**.
- **US1 (P1)** → after Foundational.
- **US2 (P2)** → after US1 (needs an invite to open).
- **US3 (P3)** → after US2 (needs both responses).
- **US4 (P1)** → after US3 (needs a generated comparison).
- **Polish (P7)** → after the stories it touches.

> Unlike a fully parallel feature, the comparison stories form a natural chain (invite → join → generate → view). Each remains **independently testable** with the prior as given-state.

### Within each story

- Tests first (must fail) → models/services → endpoints → frontend.
- Commit per task or logical group; verify CI after pushing.

### Parallel opportunities

- Setup: T002, T003 in parallel.
- Foundational: T005, T006, T008, T009, T010 in parallel (after T004).
- Within a story, `[P]` tasks (distinct files: frontend service vs. component vs. catalogs vs. tests) run together.
- Polish: T043–T047 largely parallel.

---

## Implementation Strategy

### MVP path

1. Setup + Foundational.
2. US1 (create invite) → US2 (consent + join) → US3 (generate) → US4 (view). The first end-to-end value lands when US4 completes; US1–US3 are demoable increments along the way.
3. Validate against the quickstart SC mapping, then Polish.

### Notes

- `[P]` = different files, no dependency. `[Story]` ties a task to its user story for traceability.
- Consent copy (T027/T029) and German content must pass constitution §V; comparison report text **reuses** feature 002's localized snippets — no new content authoring.
- Mutation testing on the engine stays blocked (Stryker/.NET 10, issue #18); T030 compensates with exhaustive branch + symmetry + determinism tests.
