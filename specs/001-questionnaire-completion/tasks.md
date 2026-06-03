# Tasks: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Input**: Design documents from `specs/001-questionnaire-completion/`

---

## Status Reconciliation (audited 2026-06-03)

The original checkboxes drifted from reality — work was committed in large
batches without per-task updates, so several implemented tasks were left
unchecked and some checked tasks depend on unchecked ones. This section
records the true state after a repo + git + CI audit.

**Status legend** (used throughout this file):

- `[X]` — complete to the constitution's Definition of Done (implemented, covered by tests, CI green)
- `[~]` — implemented but **not** to DoD (built as a UI mock and/or has no tests)
- `[ ]` — not started

**Key findings:**

1. **CI is green again** (run 26899162229, 2026-06-03). It was red on its
   first-and-only prior run because CI calls `npm run type-check` but no such
   script existed. The CI workflow itself only landed on 2026-06-02
   (commit 1a6ca1d) — *after* all feature code — not during Setup as T004's
   placement implies.
2. **The frontend is a disconnected design mock.** `App.tsx` drives the flow
   from hardcoded `DEMO_QUESTIONS` (2 of 46), `DEMO_REFLECTION`,
   `DEMO_PRIVATE_LINK`, and `DEMO_ACCESS_CODE`. There is **no API client**
   (T016) and **no call** to GET /api/questionnaire/current, POST /api/responses,
   /api/session/start, or /api/me/reflection. The backend US1 endpoints exist
   and are tested, but **US1 is not functional end-to-end** — the UI never
   touches the API. Frontend components are therefore marked `[~]`.
3. **Scope drift:** a `ComparisonPage` + comparison mock data were built, but
   comparison is a *future* feature (this feature only stubs the tables).
   `WelcomeStep`, `CompletionStep`, and `PageShell` were also added (not in the
   plan, but reasonable additions).
4. **Backend US1** (T017–T024, T026) is implemented and unit/integration tested;
   those boxes are now checked. **US2 backend** (T032–T034: SessionController,
   MeController) is **not started**.
5. **Still missing (constitution-required):** Stryker mutation testing (T025),
   frontend component/E2E tests (T031/T036/T039/T041/T042), coverage-threshold
   enforcement + SonarCloud quality gate, and a PR + branch-protection workflow.
   Tracked as remediation work.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create project scaffolding and CI/CD infrastructure so the solution compiles and tests can run.

- [X] T001 Create .NET solution with all 9 project files (CommonGround.sln + src/ + tests/ csproj files) in `backend/`
- [X] T002 Configure backend build tooling (global.json, Directory.Build.props, Directory.Packages.props with all NuGet package versions, NuGet.config, .editorconfig) in `backend/`
- [X] T003 [P] Scaffold React + TypeScript + Vite frontend (package.json with all dependencies, tsconfig.json, vite.config.ts, .eslintrc.json, Playwright config) in `frontend/`
- [X] T004 [P] Create GitHub Actions workflows and Dependabot config (.github/workflows/ci.yml, security.yml, release.yml, .github/dependabot.yml)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story implementation.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Create SharedKernel with domain primitives (IModule, IDomainException, Result<T>, cross-module interfaces: IQuestionnaireReader, IResponseReader, IReportingService) in `backend/src/CommonGround.SharedKernel/`
- [X] T006 [P] Create module stub projects with ModuleExtensions registration entry points (IServiceCollection extensions) in `backend/src/CommonGround.Modules.Questionnaires/`, `backend/src/CommonGround.Modules.Responses/`, `backend/src/CommonGround.Modules.Reporting/`, `backend/src/CommonGround.Modules.Privacy/`, `backend/src/CommonGround.Modules.Audit/`, `backend/src/CommonGround.Modules.Comparisons/`
- [X] T007 Create AppDbContext with EF Core entity configurations and DbSet<> properties for all entities in `backend/src/CommonGround.Api/Persistence/AppDbContext.cs`
- [X] T008 [P] Configure PostgreSQL connection string and EF Core registration (appsettings.json, appsettings.Development.json, Program.cs DI wiring) in `backend/src/CommonGround.Api/`
- [X] T009 Create initial schema migration with all tables:
  - Questionnaires: `QuestionnaireVersion`, `Question` (with SectionIndex, no Dimension), `AnswerOption` (no ScoringValue), `DimensionWeight` (AnswerOptionId + DimensionId + Weight), `DimensionMaxScore` (QuestionnaireVersionId + DimensionId + MaxScore)
  - Responses: `ResponseSet`, `Answer` (PrimaryAnswerOptionId + nullable SecondaryAnswerOptionId), unique index on `(ResponseSetId, QuestionId)`
  - Reporting: `DimensionScore` (ResponseSetId + DimensionId + RawScore + NormalisedScore), unique index on `(ResponseSetId, DimensionId)`; `InsightSnippet` (DimensionId + Text); `DimensionGroup` (GroupId + Title + OrderIndex); `DimensionGroupMembership` (DimensionGroupId + DimensionId + OrderIndex)
  - Comparisons: `ComparisonSession`, `ComparisonParticipant`
  - Audit: `AuditEvent`
  in `backend/src/CommonGround.Api/`
- [X] T010 Create seed data migration reading from repository root files (`questionary.md`, `dimensions.json`, `reflection-groups.json`):
  - One active `QuestionnaireVersion` (v1.0)
  - 46 `Question` rows across 10 sections (SectionIndex 1–10, global OrderIndex)
  - 184 `AnswerOption` rows (4 per question, OrderIndex A–D)
  - ~460 `DimensionWeight` rows from hidden mappings in `questionary.md` (each answer option carries 1–3 dimension weights; weights may be positive or negative)
  - 76 `DimensionMaxScore` rows computed from the weight table: for each dimension, sum (BestPrimaryWeight × 1.0) + (BestSecondaryWeight × 0.5) per question
  - 76 `InsightSnippet` rows from `reflection-groups.json` insights map
  - 10 `DimensionGroup` rows and 76 `DimensionGroupMembership` rows from `reflection-groups.json` groups array
  in `backend/src/CommonGround.Api/`
- [X] T011 Configure JWT HttpOnly cookie authentication middleware (cg_session cookie, HttpOnly + Secure + SameSite=Strict, 30-day Max-Age, JWT payload: sub=ResponseSetId) in `backend/src/CommonGround.Api/Program.cs`
- [X] T012 Add API security middleware (HTTPS redirect, security headers including Referrer-Policy: no-referrer, CORS restricted to frontend origin, rate limiting on POST endpoints) in `backend/src/CommonGround.Api/Program.cs`
- [X] T013 Create NetArchTest architecture boundary rules (no Modules.X → Modules.Y references; only CommonGround.Api may reference all modules; all cross-module access via SharedKernel interfaces) in `backend/tests/CommonGround.ArchitectureTests/`
- [X] T014 [P] Set up Testcontainers integration test base class (PostgreSQL container + WebApplicationFactory with test database) in `backend/tests/CommonGround.IntegrationTests/`
- [X] T015 [P] Create API types shared file reflecting current contracts:
  - `GetQuestionnaireResponse` (id, versionNumber, questions with sectionIndex + orderIndex + answerOptions)
  - `SubmitResponseRequest` (answers array: questionId + primaryAnswerOptionId + secondaryAnswerOptionId?)
  - `SubmitResponseResult` (privateResultLink, accessCode, reflection)
  - `ReflectionDto` (groups array)
  - `ReflectionGroupDto` (id, title, insights array)
  - `InsightDto` (dimensionId, text, strength 1–5)
  - `GetMyReflectionResponse` (reflection, accessCodeAvailable)
  in `frontend/src/types/api.ts`
- [ ] T016 [P] Create questionnaire API service client (fetchCurrentQuestionnaire, submitResponses, startSession, fetchMyReflection — all typed against api.ts) in `frontend/src/services/questionnaireApi.ts`

**Checkpoint**: Foundation ready — all module stubs compile, schema migrates, seed data loads, architecture tests pass.

---

## Phase 3: User Story 1 — Complete questionnaire and receive personal reflection (Priority: P1) 🎯 MVP

**Goal**: A user consents, completes all 46 questions one at a time across 10 sections, submits, and receives a grouped personal reflection page with insight snippets, a private result link, and an access code.

**Independent Test**: Open app → acknowledge consent → answer all questions (use Next/Back) → submit → verify personal reflection shows grouped insights (at least one group, each insight has text and strength indicator), private result link, and access code are all shown.

### Implementation for User Story 1

- [X] T017 [P] [US1] Implement Questionnaire module entities (QuestionnaireVersion, Question, AnswerOption, DimensionWeight, DimensionMaxScore) with EF Core configurations and IQuestionnaireReader implementation (GetActiveVersion, GetDimensionWeightsForOptions, GetDimensionMaxScores) in `backend/src/CommonGround.Modules.Questionnaires/`
- [X] T017b [P] [US1] Implement Reporting module entities (DimensionScore, InsightSnippet, DimensionGroup, DimensionGroupMembership) with EF Core configurations and IReportingService interface in `backend/src/CommonGround.Modules.Reporting/`
- [X] T018 [P] [US1] Implement Responses module entities (ResponseSet, Answer with PrimaryAnswerOptionId + nullable SecondaryAnswerOptionId) with EF Core configurations and IResponseRepository + IResponseReader implementations in `backend/src/CommonGround.Modules.Responses/`
- [X] T019 [P] [US1] Implement Privacy module (token generation via RandomNumberGenerator.GetBytes(32) + Base64url encoding, HMAC-SHA256 hashing with server secret, access code XXXX-XXXX-XXXX formatting) in `backend/src/CommonGround.Modules.Privacy/`
- [X] T020 [P] [US1] Implement Audit module (AuditEvent entity, IAuditLogger interface, EF Core append-only implementation — no raw answers/tokens in Metadata) in `backend/src/CommonGround.Modules.Audit/`
- [X] T021 [US1] Implement GET /api/questionnaire/current endpoint (query active QuestionnaireVersion; return questions ordered by OrderIndex with sectionIndex and answer options; dimension weights excluded from response) in `backend/src/CommonGround.Api/Controllers/QuestionnaireController.cs`
- [X] T022 [US1] Implement deterministic scoring engine:
  - For each Answer: accumulate DimensionWeight contributions (Primary × 1.0, Secondary × 0.5 if present)
  - Normalise each raw score: NormalisedScore = RawScore / DimensionMaxScore (clamp 0.0–1.0)
  - Persist one DimensionScore row per dimension per ResponseSet
  in `backend/src/CommonGround.Modules.Reporting/ScoringEngine.cs`
- [X] T022b [US1] Implement reflection assembly service:
  - Load DimensionGroups and DimensionGroupMemberships (ordered)
  - For each group, collect DimensionScores where NormalisedScore ≥ 0.4 (display threshold)
  - For each qualifying dimension, fetch InsightSnippet.Text and compute strength = max(1, min(5, ceil(NormalisedScore × 5)))
  - Omit groups with no qualifying dimensions
  - Return grouped ReflectionDto
  in `backend/src/CommonGround.Modules.Reporting/ReflectionAssembler.cs`
- [X] T023 [US1] Implement POST /api/responses endpoint:
  - Validate: all 46 questions answered, no duplicate questionIds, each primaryAnswerOptionId belongs to its questionId, secondaryAnswerOptionId (if present) belongs to same question and differs from primary
  - Create ResponseSet + Answer rows
  - Run scoring engine (T022) → persist DimensionScore rows
  - Assemble reflection (T022b) → grouped ReflectionDto
  - Generate private result token + access code via Privacy module
  - Log questionnaire_completed + personal_reflection_generated audit events
  - Return 201 with reflection, privateResultLink=/me#TOKEN, accessCode
  in `backend/src/CommonGround.Api/Controllers/ResponsesController.cs`
- [X] T024 [US1] Unit tests for scoring engine and reflection assembler:
  - Scoring: primary-only answers accumulate at ×1.0; secondary contributions at ×0.5; negative weights reduce score; normalisation produces 0.0–1.0; same inputs produce same output
  - Reflection assembly: dimensions below threshold (< 0.4) are excluded; groups with no qualifying dimensions are omitted; strength maps correctly (e.g. 0.4 → 1, 1.0 → 5); insight text matches InsightSnippet for dimension
  - Edge cases: dimension with max score 0 handled safely; all answers secondary-only handled
  in `backend/tests/CommonGround.UnitTests/Reporting/`
- [~] T025 [US1] Stryker.NET mutation test configuration targeting scoring engine and reflection assembler in `backend/stryker-config.json` — config + tool manifest + classic-format `backend/Stryker.sln` + manual `mutation.yml` workflow all committed and ready, scoped to `ScoringEngine.cs` + `ReflectionAssembler.cs` with break threshold 60. **Blocked from running** by Stryker.NET 4.14.2 (latest, incl. prerelease) lacking .NET 10 SDK support (Buildalyzer returns 0 analyzable projects; cannot read `.slnx`). Promote to a required PR check once a compatible Stryker ships.
- [X] T026 [US1] Integration tests for GET /api/questionnaire/current (200 with questions including sectionIndex, no dimension weights in response) and POST /api/responses:
  - Valid submission → 201 with grouped reflection (≥1 group, each insight has text + strength 1–5)
  - Missing questions → 400 incomplete_answers
  - Duplicate questionIds → 400
  - Invalid answerOptionId → 400
  - secondaryAnswerOptionId same as primary → 400
  - secondaryAnswerOptionId from different question → 400
  in `backend/tests/CommonGround.IntegrationTests/QuestionnaireFlowTests.cs`
- [X] T027 [P] [US1] Implement ConsentStep component (privacy and consent explanation text, acknowledge button, cannot proceed until acknowledged) in `frontend/src/components/ConsentStep/`
- [X] T028 [P] [US1] Implement QuestionStep component (question text, 4 answer options as radio-style cards with primary selection; optional secondary selection from remaining options once primary chosen; Next/Back buttons; selected options highlighted) in `frontend/src/components/QuestionStep/`
- [X] T029 [P] [US1] Implement ProgressIndicator component (global "X of 46" label with visual progress bar; section label "Section Y of 10") in `frontend/src/components/ProgressIndicator/`
- [~] T030 [US1] Implement QuestionnairePage with step orchestration:
  - Fetch questionnaire on mount (GET /api/questionnaire/current)
  - Steps: [ConsentStep, QuestionStep×46, CredentialsDisplayStep]
  - currentStepIndex state, answers map (questionId → {primaryAnswerOptionId, secondaryAnswerOptionId?})
  - All 46 primary answers required before submit enabled
  - POST /api/responses on final submit; pass reflection + credentials to CredentialsDisplayStep
  in `frontend/src/pages/QuestionnairePage/`
- [X] T031 [P] [US1] Component tests: ConsentStep blocks proceed until acknowledged; QuestionStep selects primary answer and calls onNext; QuestionStep allows optional secondary selection from remaining options; ProgressIndicator shows correct fraction and section label in `frontend/tests/components/`

**Checkpoint**: Full questionnaire flow functional — user can complete all 46 questions and credentials are displayed.

---

## Phase 4: User Story 2 — View personal reflection via private result link (Priority: P2)

**Goal**: A returning user navigates to their saved `/me#TOKEN` link and sees their grouped personal reflection.

**Independent Test**: Complete questionnaire → copy private result link → open new tab → paste link → verify reflection page loads with grouped insights matching original completion.

### Implementation for User Story 2

- [ ] T032 [US2] Implement POST /api/session/start endpoint (HMAC-SHA256 hash incoming token; look up ResponseSet by PrivateResultTokenHash; issue JWT cg_session cookie (sub=ResponseSetId, exp=30 days); return 200 empty body; return 401 if not found or IsDeleted=true) in `backend/src/CommonGround.Api/Controllers/SessionController.cs`
- [ ] T033 [US2] Implement GET /api/me/reflection endpoint (require cg_session cookie; extract ResponseSetId from JWT sub; load DimensionScores for ResponseSet; assemble grouped ReflectionDto via reflection assembler (T022b); return grouped reflection + accessCodeAvailable=true if AccessCodeHash present; return 401 if no valid cookie; return 404 if IsDeleted=true) in `backend/src/CommonGround.Api/Controllers/MeController.cs`
- [ ] T034 [US2] Integration tests for POST /api/session/start (valid token → 200 + cg_session cookie set; invalid token → 401) and GET /api/me/reflection (with cookie → 200 + grouped reflection with ≥1 group; without cookie → 401; soft-deleted response → 404) in `backend/tests/CommonGround.IntegrationTests/ReflectionAccessTests.cs`
- [~] T035 [US2] Implement ReflectionPage:
  - Read token from window.location.hash on mount
  - POST /api/session/start with token
  - GET /api/me/reflection
  - Render grouped reflection: each group as a section with title, each insight as text + 5-dot strength indicator
  - Show "result not available" on 401/404
  - Raw token cleared from component state after session established
  in `frontend/src/pages/ReflectionPage/`
- [ ] T035b [P] [US2] Implement InsightCard component (insight text + 5-dot strength indicator where filled dots = strength value) in `frontend/src/components/InsightCard/`
- [ ] T036 [US2] Component tests for ReflectionPage and InsightCard: ReflectionPage renders group titles and insights on success; shows not-available message on 401; InsightCard renders correct number of filled dots for each strength value 1–5 in `frontend/tests/components/`

**Checkpoint**: Returning users can access their grouped personal reflection via their saved private result link.

---

## Phase 5: User Story 3 — Understand credentials after completion (Priority: P2)

**Goal**: The credentials screen clearly distinguishes the two credentials with separate labels, explanations, and a privacy warning so users know what to save and why.

**Independent Test**: Complete questionnaire → credentials screen shows private result link with "bookmark this to return to your results" explanation AND access code with "use this to reuse your response in a future comparison — keep it private" explanation AND a privacy warning.

### Implementation for User Story 3

- [~] T037 [US3] Implement CredentialsDisplay component: private result link section (label, clickable link, "bookmark this link to return to your results" explanation, copy-to-clipboard button); access code section (label, formatted XXXX-XXXX-XXXX display, "use this code to reuse your response in a future comparison — not for opening this page" explanation); shared privacy warning ("keep your access code private — anyone with it can reuse your response") in `frontend/src/components/CredentialsDisplay/`
- [~] T038 [US3] Integrate CredentialsDisplay as the final step in QuestionnairePage after successful POST /api/responses (pass privateResultLink and accessCode from the 201 response body) in `frontend/src/pages/QuestionnairePage/`
- [X] T039 [US3] Component tests for CredentialsDisplay: both sections present with distinct labels; access code explanation mentions future comparison; private result link explanation mentions bookmarking; privacy warning text present; copy button copies the link to clipboard in `frontend/tests/components/CredentialsDisplay.test.tsx`

**Checkpoint**: All 3 user stories independently functional — credentials screen is clear and unambiguous.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility compliance, end-to-end test coverage, and final verification.

- [ ] T040 [P] WCAG 2.1 AA audit: add aria-labels to all interactive elements (radio inputs, primary/secondary answer cards, buttons, progress indicator, strength dots); verify keyboard navigation through full questionnaire flow; confirm sufficient color contrast ratios in `frontend/src/`
- [ ] T041 [P] E2E test (Playwright): happy path — consent acknowledged → all 46 questions answered with Next/Back navigation (including one secondary answer) → submit → credentials displayed with link and code in `frontend/tests/e2e/questionnaire.spec.ts`
- [ ] T042 [P] E2E test (Playwright): private result link return — complete questionnaire → copy private result link → open new tab → navigate to `/me#TOKEN` → reflection page loads with grouped insights in `frontend/tests/e2e/reflection-access.spec.ts`
- [ ] T043 Run quickstart.md verification steps (docker compose up, dotnet ef database update, dotnet run, npm run dev) and confirm all verification steps pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2
- **User Story 2 (Phase 4)**: Depends on Phase 2 + Phase 3 (session/start and me/reflection need a ResponseSet + DimensionScores created by US1)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (CredentialsDisplay integrates into QuestionnairePage built in T030)
- **Polish (Phase 6)**: Depends on Phases 3–5 complete

### Within Phase 3 (User Story 1)

- T017, T017b, T018, T019, T020 are independent — run in parallel
- T021 depends on T017 (needs Questionnaire entities)
- T022 depends on T017, T018 (needs DimensionWeight + Answer)
- T022b depends on T017b, T022 (needs DimensionScore + InsightSnippet + DimensionGroup)
- T023 depends on T022 + T022b + T019 + T020
- T024 depends on T022 + T022b
- T026 depends on T023
- T027, T028, T029 are independent — run in parallel
- T030 depends on T027, T028, T029
- T031 depends on T027, T028, T029

### Within Phase 4 (User Story 2)

- T032 and T033 depend on T022b (reflection assembler reused in T033)
- T035 and T035b are independent — run in parallel
- T036 depends on T035 and T035b

---

## Parallel Opportunities

### Phase 3 (User Story 1)

```
# Five module implementations run in parallel:
T017  — Questionnaire module entities + IQuestionnaireReader
T017b — Reporting module entities + IReportingService
T018  — Responses module entities + IResponseReader
T019  — Privacy module (token + hash + code generation)
T020  — Audit module (AuditEvent + IAuditLogger)

# Three frontend components run in parallel:
T027  — ConsentStep
T028  — QuestionStep (with primary + optional secondary selection)
T029  — ProgressIndicator
```

### Phase 4 (User Story 2)

```
# Backend and frontend can proceed in parallel once API contract is clear:
T032 + T033 — Backend endpoints (SessionController, MeController)
T035 + T035b — ReflectionPage + InsightCard (develop against mock API)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational — CRITICAL, blocks everything
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Consent → 46 questions → submit → see grouped reflection + credentials
5. Merge to main

### Incremental Delivery

1. Setup + Foundational → solution builds, DB migrates, seed loads
2. User Story 1 → complete questionnaire + grouped reflection → **demo-ready MVP**
3. User Story 2 → returning users via saved link
4. User Story 3 → credentials screen fully explained
5. Polish → E2E coverage + accessibility + quickstart verification

---

## Notes

- `[P]` = can run in parallel (different files, no incomplete dependencies)
- `[US1/US2/US3]` = user story label for traceability to spec.md
- Dimension weights are never returned in API responses — enforced in T021; verified in T026
- Audit events must not contain raw answers, tokens, or access codes — enforced in T020 and T023; verified in T026
- Private result token is delivered via URL fragment only — enforced in T023 (privateResultLink=/me#TOKEN); verified in T035 (reads window.location.hash)
- JWT cookie must be HttpOnly; Secure; SameSite=Strict — enforced in T011 and T032; verified in T034
- Display threshold (0.4) and strength formula (ceil(score × 5)) are constants in the Reporting module — not hardcoded in the API layer
- Seed data (T010) reads from `questionary.md`, `dimensions.json`, and `reflection-groups.json` at migration time — not at runtime
- Commit after each task or logical group
