# Tasks: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Input**: Design documents from `specs/001-questionnaire-completion/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create project scaffolding and CI/CD infrastructure so the solution compiles and tests can run.

- [ ] T001 Create .NET solution with all 9 project files (CommonGround.sln + src/ + tests/ csproj files) in `backend/`
- [ ] T002 Configure backend build tooling (global.json, Directory.Build.props, Directory.Packages.props with all NuGet package versions, NuGet.config, .editorconfig) in `backend/`
- [ ] T003 [P] Scaffold React + TypeScript + Vite frontend (package.json with all dependencies, tsconfig.json, vite.config.ts, .eslintrc.json, Playwright config) in `frontend/`
- [ ] T004 [P] Create GitHub Actions workflows and Dependabot config (.github/workflows/ci.yml, security.yml, release.yml, .github/dependabot.yml)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story implementation.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Create SharedKernel with domain primitives (IModule, IDomainException, Result<T>, cross-module interfaces: IQuestionnaireReader, IResponseReader) in `backend/src/CommonGround.SharedKernel/`
- [ ] T006 [P] Create module stub projects with ModuleExtensions registration entry points (IServiceCollection extensions) in `backend/src/CommonGround.Modules.Questionnaires/`, `backend/src/CommonGround.Modules.Responses/`, `backend/src/CommonGround.Modules.Reporting/`, `backend/src/CommonGround.Modules.Privacy/`, `backend/src/CommonGround.Modules.Audit/`, `backend/src/CommonGround.Modules.Comparisons/`
- [ ] T007 Create AppDbContext with EF Core entity configurations and DbSet<> properties for all entities in `backend/src/CommonGround.Api/Persistence/AppDbContext.cs`
- [ ] T008 [P] Configure PostgreSQL connection string and EF Core registration (appsettings.json, appsettings.Development.json, Program.cs DI wiring) in `backend/src/CommonGround.Api/`
- [ ] T009 Create initial schema migration (all tables: QuestionnaireVersion, Question, AnswerOption, ScoringRule, InsightTemplate, ResponseSet, Answer, ComparisonSession, ComparisonParticipant, AuditEvent — including unique index on Answer(ResponseSetId, QuestionId)) in `backend/src/CommonGround.Api/`
- [ ] T010 Create seed data migration: QuestionnaireVersion v1.0 with 12 questions (2 per dimension × 6 dimensions), 48 answer options with ScoringValues 1–4, 6 ScoringRules, 18 InsightTemplates (3 score bands × 6 dimensions, InsightType=PersonalReflection) in `backend/src/CommonGround.Api/`
- [ ] T011 Configure JWT HttpOnly cookie authentication middleware (cg_session cookie, HttpOnly + Secure + SameSite=Strict, 30-day Max-Age, JWT payload: sub=ResponseSetId) in `backend/src/CommonGround.Api/Program.cs`
- [ ] T012 Add API security middleware (HTTPS redirect, security headers including Referrer-Policy: no-referrer, CORS restricted to frontend origin, rate limiting on POST endpoints) in `backend/src/CommonGround.Api/Program.cs`
- [ ] T013 Create NetArchTest architecture boundary rules (no Modules.X → Modules.Y references; only CommonGround.Api may reference all modules; all cross-module access via SharedKernel interfaces) in `backend/tests/CommonGround.ArchitectureTests/`
- [ ] T014 [P] Set up Testcontainers integration test base class (PostgreSQL container + WebApplicationFactory with test database) in `backend/tests/CommonGround.IntegrationTests/`
- [ ] T015 [P] Create API types shared file (QuestionnaireDto, SubmitResponseRequest, SubmitResponseResult, ReflectionDto, InsightDto) in `frontend/src/types/api.ts`
- [ ] T016 [P] Create questionnaire API service client (fetchCurrentQuestionnaire, submitResponses, startSession, fetchMyReflection — all with typed request/response) in `frontend/src/services/questionnaireApi.ts`

**Checkpoint**: Foundation ready — all module stubs compile, schema migrates, seed data loads, architecture tests pass.

---

## Phase 3: User Story 1 — Complete questionnaire and receive personal reflection (Priority: P1) 🎯 MVP

**Goal**: A user consents, completes all 12 questions one at a time, submits, and receives a personal reflection page with insights, a private result link, and an access code.

**Independent Test**: Open app → acknowledge consent → answer all 12 questions (use Next/Back) → submit → verify personal reflection with ≥1 insight, private result link, and access code are all shown.

### Implementation for User Story 1

- [ ] T017 [P] [US1] Implement Questionnaire module entities (QuestionnaireVersion, Question, AnswerOption, ScoringRule, InsightTemplate) with EF Core configurations and IQuestionnaireReader implementation in `backend/src/CommonGround.Modules.Questionnaires/`
- [ ] T018 [P] [US1] Implement Responses module entities (ResponseSet, Answer) with EF Core configurations and IResponseRepository + IResponseReader implementations in `backend/src/CommonGround.Modules.Responses/`
- [ ] T019 [P] [US1] Implement Privacy module (token generation via RandomNumberGenerator.GetBytes(32) + Base64url encoding, HMAC-SHA256 hashing with server secret, access code XXXX-XXXX-XXXX formatting) in `backend/src/CommonGround.Modules.Privacy/`
- [ ] T020 [P] [US1] Implement Audit module (AuditEvent entity, IAuditLogger interface, EF Core append-only implementation — no raw answers/tokens in Metadata) in `backend/src/CommonGround.Modules.Audit/`
- [ ] T021 [US1] Implement GET /api/questionnaire/current endpoint (query active QuestionnaireVersion, return questions + answer options ordered by OrderIndex, ScoringValue excluded from response) in `backend/src/CommonGround.Api/Controllers/QuestionnaireController.cs`
- [ ] T022 [US1] Implement deterministic scoring engine (consume IQuestionnaireReader + IResponseReader; group answers by Dimension; sum ScoringValues per dimension; range-lookup InsightTemplate where InsightType=PersonalReflection and MinScore ≤ score ≤ MaxScore) in `backend/src/CommonGround.Modules.Reporting/`
- [ ] T023 [US1] Implement POST /api/responses endpoint (validate: all questions answered, no duplicate questionIds, each answerOptionId belongs to its questionId; create ResponseSet + Answers; run scoring engine; generate token + access code via Privacy module; log questionnaire_completed + personal_reflection_generated audit events; return 201 with reflection, privateResultLink=/me#TOKEN, accessCode) in `backend/src/CommonGround.Api/Controllers/ResponsesController.cs`
- [ ] T024 [US1] Unit tests for scoring engine: all 6 dimensions, all 3 score bands (low 2–4, mid 5–6, high 7–8), edge cases at band boundaries, missing answer rejection in `backend/tests/CommonGround.UnitTests/Reporting/ScoringEngineTests.cs`
- [ ] T025 [US1] Stryker.NET mutation test configuration targeting scoring engine in `backend/stryker-config.json`
- [ ] T026 [US1] Integration tests for GET /api/questionnaire/current (200 with questions, no ScoringValue in response) and POST /api/responses (valid → 201 with reflection, missing answers → 400, duplicate questionIds → 400, invalid answerOptionId → 400) in `backend/tests/CommonGround.IntegrationTests/QuestionnaireFlowTests.cs`
- [ ] T027 [P] [US1] Implement ConsentStep component (privacy and consent explanation text, acknowledge button, cannot proceed until acknowledged) in `frontend/src/components/ConsentStep/`
- [ ] T028 [P] [US1] Implement QuestionStep component (question text, 4 radio-style answer options, Next/Back buttons, selected option highlighted) in `frontend/src/components/QuestionStep/`
- [ ] T029 [P] [US1] Implement ProgressIndicator component ("X of 12" label with visual progress bar) in `frontend/src/components/ProgressIndicator/`
- [ ] T030 [US1] Implement QuestionnairePage with step orchestration (fetch questionnaire on mount; step array: [ConsentStep, QuestionStep×12, CredentialsDisplayStep]; currentStepIndex state; collect answers map; disable submit until all answered; POST /api/responses on final submit; pass reflection + credentials to final step) in `frontend/src/pages/QuestionnairePage/`
- [ ] T031 [P] [US1] Component tests: ConsentStep blocks proceed until acknowledged; QuestionStep selects answer and calls onNext/onBack; ProgressIndicator shows correct fraction in `frontend/tests/components/`

**Checkpoint**: Full questionnaire flow functional — user can complete all 12 questions and credentials are displayed.

---

## Phase 4: User Story 2 — View personal reflection via private result link (Priority: P2)

**Goal**: A returning user navigates to their saved `/me#TOKEN` link and sees their personal reflection.

**Independent Test**: Complete questionnaire → copy private result link → open new tab → paste link → verify reflection page loads with the same working-style insights.

### Implementation for User Story 2

- [ ] T032 [US2] Implement POST /api/session/start endpoint (HMAC-SHA256 hash the incoming token; look up ResponseSet by PrivateResultTokenHash; issue JWT cg_session cookie (sub=ResponseSetId, exp=30 days); return 200 empty body on success; return 401 if not found or IsDeleted=true) in `backend/src/CommonGround.Api/Controllers/SessionController.cs`
- [ ] T033 [US2] Implement GET /api/me/reflection endpoint (require cg_session cookie; extract ResponseSetId from JWT sub; re-run scoring engine via Reporting module; return reflection insights + accessCodeAvailable=true if AccessCodeHash present; return 401 if no valid cookie; return 404 if IsDeleted=true) in `backend/src/CommonGround.Api/Controllers/MeController.cs`
- [ ] T034 [US2] Integration tests for POST /api/session/start (valid token → 200 + cg_session cookie set, invalid token → 401) and GET /api/me/reflection (with cookie → 200 + insights, without cookie → 401, soft-deleted response → 404) in `backend/tests/CommonGround.IntegrationTests/ReflectionAccessTests.cs`
- [ ] T035 [US2] Implement ReflectionPage (read token from window.location.hash on mount; POST /api/session/start with token; GET /api/me/reflection; render insight list; show "result not available" message on 401/404; raw token cleared from component state after session is established) in `frontend/src/pages/ReflectionPage/`
- [ ] T036 [US2] Component tests for ReflectionPage: renders insights list on success, shows not-available message on 401, shows not-found message on 404 in `frontend/tests/components/ReflectionPage.test.tsx`

**Checkpoint**: Returning users can access their personal reflection via their saved private result link.

---

## Phase 5: User Story 3 — Understand credentials after completion (Priority: P2)

**Goal**: The credentials screen clearly distinguishes the two credentials with separate labels, explanations, and a privacy warning so users know what to save and why.

**Independent Test**: Complete questionnaire → credentials screen shows private result link with "bookmark this to return to your results" explanation AND access code with "use this to share your response in a future comparison — keep it private" explanation AND a privacy warning.

### Implementation for User Story 3

- [ ] T037 [US3] Implement CredentialsDisplay component: private result link section (label, clickable link, "bookmark this link to return to your results" explanation, copy-to-clipboard button); access code section (label, formatted XXXX-XXXX-XXXX display, "use this code to reuse your response in a future comparison — not for opening this page" explanation); shared privacy warning ("keep your access code private — anyone with it can reuse your response") in `frontend/src/components/CredentialsDisplay/`
- [ ] T038 [US3] Integrate CredentialsDisplay as the final step in QuestionnairePage after successful POST /api/responses (pass privateResultLink and accessCode from the 201 response body) in `frontend/src/pages/QuestionnairePage/`
- [ ] T039 [US3] Component tests for CredentialsDisplay: both sections present with distinct labels; access code explanation mentions future comparison; private result link explanation mentions bookmarking; privacy warning text present; copy button copies the link to clipboard in `frontend/tests/components/CredentialsDisplay.test.tsx`

**Checkpoint**: All 3 user stories independently functional — credentials screen is clear and unambiguous.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility compliance, end-to-end test coverage, and final verification.

- [ ] T040 [P] WCAG 2.1 AA audit: add aria-labels to all interactive elements (radio inputs, buttons, progress indicator); verify keyboard navigation through full questionnaire flow; confirm sufficient color contrast ratios in `frontend/src/`
- [ ] T041 [P] E2E test (Playwright): happy path — consent acknowledged → 12 questions answered with Next/Back navigation → submit → credentials displayed with link and code in `frontend/tests/e2e/questionnaire.spec.ts`
- [ ] T042 [P] E2E test (Playwright): private result link return — complete questionnaire → copy private result link → open new tab → navigate to `/me#TOKEN` → reflection page loads with same insights in `frontend/tests/e2e/reflection-access.spec.ts`
- [ ] T043 Run quickstart.md verification steps (docker compose up, dotnet ef database update, dotnet run, npm run dev) and confirm all 7 verification steps pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 — no dependency on other user stories
- **User Story 2 (Phase 4)**: Depends on Phase 2 + Phase 3 (session/start and me/reflection need a ResponseSet created by US1)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (CredentialsDisplay integrates into QuestionnairePage built in T030)
- **Polish (Phase 6)**: Depends on Phases 3–5 complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — foundation of the entire feature
- **US2 (P2)**: Backend endpoints can be built after Phase 2; integration tests depend on US1 to create a ResponseSet
- **US3 (P2)**: CredentialsDisplay (T037) must be done before QuestionnairePage integration (T038); implement after T030

### Within Each User Story

- Module entities (T017–T020) before endpoint handlers (T021–T023)
- Scoring engine (T022) before POST /api/responses handler (T023)
- Frontend components (T027–T029) before QuestionnairePage orchestration (T030)
- CredentialsDisplay (T037) before QuestionnairePage integration (T038)

---

## Parallel Opportunities

### Phase 3 (User Story 1)

```
# Four module implementations run in parallel:
T017 — Questionnaire module entities + IQuestionnaireReader
T018 — Responses module entities + IResponseReader
T019 — Privacy module (token + hash + code generation)
T020 — Audit module (AuditEvent + IAuditLogger)

# Three frontend components run in parallel:
T027 — ConsentStep
T028 — QuestionStep
T029 — ProgressIndicator
```

### Phase 4 (User Story 2)

```
# Backend and frontend can proceed in parallel once API contract is clear:
T032 + T033 — Backend endpoints (SessionController, MeController)
T035 — ReflectionPage (develop against mock API)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks everything)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Consent → 12 questions → submit → see reflection + credentials
5. Merge to main

### Incremental Delivery

1. Setup + Foundational → solution builds, DB migrates
2. User Story 1 → complete questionnaire + reflection → **demo-ready MVP**
3. User Story 2 → returning users via saved link
4. User Story 3 → credentials screen fully explained
5. Polish → E2E coverage + accessibility + quickstart verification

---

## Notes

- `[P]` = can run in parallel (different files, no incomplete dependencies)
- `[US1/US2/US3]` = user story label for traceability to spec.md
- ScoringValue is never returned in API responses — enforced in T021; verified in T026
- Audit events must not contain raw answers, tokens, or access codes — enforced in T020 and T023; verified in T026
- Private result token is delivered via URL fragment only — enforced in T023 (privateResultLink=/me#TOKEN); verified in T035 (reads window.location.hash)
- JWT cookie must be HttpOnly; Secure; SameSite=Strict — enforced in T011 and T032; verified in T034
- Commit after each task or logical group
