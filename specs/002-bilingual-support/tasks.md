---
description: "Task list for 002-bilingual-support"
---

# Tasks: Bilingual (English/German) Support

**Input**: Design documents from `specs/002-bilingual-support/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/api.md](contracts/api.md)

**Tests**: Included — the constitution mandates TDD (tests before implementation) and a CI quality gate. Write each test task first and confirm it FAILS before the implementation task.

**Commit discipline**: One commit per task (or tight logical group), task ID in the subject — e.g. `feat(backend): T008 SupportedLocales helper (en|de, default en)`. Push and confirm CI is green before moving on. No `Co-Authored-By: Claude` trailer. TDD order is non-negotiable.

**Locale convention**: `en` is default + field-level fallback; `de` lives in translation rows; `?locale=` is the transport (unknown → `en`). Scoring never depends on locale.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: US1–US4 (user-story phases only)

---

## Phase 1: Setup

**Purpose**: Decision record + test scaffolding that the rest of the work leans on.

- [x] T001 Write ADR-0008 (localization schema: per-locale translation tables with English base/fallback; locale-first net-new `DimensionTitle`; `?locale=` transport; no locale stored on `ResponseSet`) in `docs/adr/0008-localization-translation-tables.md` and link it in `docs/adr/README.md` (#21)
- [ ] T002 [P] Create bilingual test fixtures (small EN+DE sample rows for questions, options, group titles, insight texts, dimension titles) for backend integration tests in `backend/tests/CommonGround.IntegrationTests/Fixtures/` and frontend in `frontend/tests/fixtures/` — so code/tests do not block on final German copy (#22)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema, locale plumbing, and frontend i18n infrastructure shared by every story.

**⚠️ CRITICAL**: No user story work begins until this phase is complete.

### Backend schema

- [x] T003 [P] Create `QuestionTranslation` and `AnswerOptionTranslation` entities (Id, FK, Locale, Text) in `backend/src/CommonGround.Modules.Questionnaires/Entities/` (#23)
- [x] T004 [P] Create `InsightSnippetTranslation`, `DimensionGroupTranslation`, and net-new `DimensionTitle` (DimensionId, Locale, Title) entities in `backend/src/CommonGround.Modules.Reporting/Entities/` (#24)
- [x] T005 [P] Add EF Core configurations with unique `(EntityId, Locale)` indexes for the five translation entities in `backend/src/CommonGround.Api/Persistence/Configurations/` (#25)
- [x] T006 Register the new entities on `AppDbContext` and create EF Core migration `AddLocalizationTranslations` (schema for the five tables; `SeedUp`/`SeedDown` seed-helper stubs) in `backend/src/CommonGround.Api/Persistence/Migrations/` (depends on T003–T005) (#26)
- [x] T007 Architecture test asserting the new translation entities stay within their owning modules (Questionnaires / Reporting) and are reached only via SharedKernel interfaces, in `backend/tests/CommonGround.ArchitectureTests/` (#27)

### Backend locale resolution

- [x] T008 [P] Unit test for locale resolution (`en`/`de` accepted, default `en`, unknown/empty → `en`) in `backend/tests/CommonGround.UnitTests/` — write first, must FAIL (#28)
- [x] T009 Implement `SupportedLocales` helper (accepted set, default, normalise/validate) in `backend/src/CommonGround.SharedKernel/Localization/SupportedLocales.cs` (moved from Api so unit tests can reference it without the web host) — make T008 pass (#29)

### Frontend i18n infrastructure

- [x] T010 [P] Component test for `LanguageContext` (default `en`, persists to `localStorage`, sets `document.documentElement.lang`) in `frontend/tests/components/` — write first, must FAIL (#30)
- [x] T011 Implement `LanguageContext` + provider in `frontend/src/i18n/LanguageContext.tsx` — make T010 pass (#31)
- [x] T012 [P] Add typed message catalogs `messages.en.ts` + `messages.de.ts` and a `useMessages` hook (TS strict: identical key sets enforced) in `frontend/src/i18n/` (#32)
- [x] T013 [P] Component test for `LanguageSwitcher` (renders both locales, keyboard-operable, `aria-label`, invokes locale change) in `frontend/tests/components/` — write first, must FAIL (#33)
- [x] T014 Implement accessible `LanguageSwitcher` in `frontend/src/components/LanguageSwitcher/LanguageSwitcher.tsx` — make T013 pass (#34)
- [x] T015 Extend the API client to send `?locale=<current>` on content calls in `frontend/src/services/questionnaireApi.ts` (#35)

**Checkpoint**: schema migrates, locale resolves with fallback, and the frontend can hold/switch a locale. User stories can begin.

---

## Phase 3: User Story 1 - Take the questionnaire and read the reflection in my language (Priority: P1) 🎯 MVP

**Goal**: Complete consent → questionnaire → reflection entirely in the chosen language (questions, options, group titles, insight text, and UI chrome). Per-insight titles arrive in US3.

**Independent Test**: Choose German at the start, walk the full flow, and confirm 100% of visible text is German with no English leakage; repeat in English. Identical answers in en vs de yield identical insights/order/strength.

### Tests for User Story 1 (write first, must FAIL)

- [x] T016 [P] [US1] Integration test: `GET /api/questionnaire/current?locale=de` returns German question/option text with **identical IDs and order** to `en` (Testcontainers) in `backend/tests/CommonGround.IntegrationTests/` (#36)
- [x] T017 [P] [US1] Integration test: `POST /api/responses?locale=de` returns reflection with German group titles + insight text, and the **same insights/order/strength** as `en` for identical answers (SC-003) in `backend/tests/CommonGround.IntegrationTests/` (#37)
- [x] T018 [P] [US1] Frontend test: full questionnaire + chrome (ConsentStep, ProgressIndicator, ReflectionPage) render in German when locale = `de` in `frontend/tests/components/` (#38)

### Implementation for User Story 1

- [x] T019 [US1] Questionnaire reader returns localized question/option text by locale with English fallback in `backend/src/CommonGround.Modules.Questionnaires/` (reader + SharedKernel DTO as needed) (#39)
- [x] T020 [US1] `QuestionnaireController` accepts `?locale=`, validates via `SupportedLocales`, passes to reader in `backend/src/CommonGround.Api/Controllers/QuestionnaireController.cs` (#40)
- [x] T021 [US1] `ReflectionAssembler` takes a `locale` param and localizes **insight text + group title** with English fallback; update `IReportingService.AssembleReflectionAsync` signature in `backend/src/CommonGround.Modules.Reporting/ReflectionAssembler.cs` and `backend/src/CommonGround.SharedKernel/Interfaces/IReportingService.cs` (per-insight `Title` added in US3) (#41)
- [x] T022 [US1] `ResponsesController` accepts `?locale=` and passes it to the assembler in `backend/src/CommonGround.Api/Controllers/ResponsesController.cs` (#42)
- [x] T023 [US1] Seed DE rows for questions/options (from `questionary_german.md`) and group titles + insight texts (from authored DE content; placeholder/fixture until finalized — see T045) into the `AddLocalizationTranslations` seed helper in `backend/src/CommonGround.Api/Persistence/Migrations/` (#43)
- [x] T024 [P] [US1] Mount `LanguageSwitcher` on the consent and question pages and make the questionnaire fetch use the current locale in `frontend/src/pages/QuestionnairePage/` (#44)
- [x] T025 [P] [US1] Localize UI chrome via `useMessages` in `ConsentStep`, `ProgressIndicator`, `QuestionStep`, `CredentialsDisplay`, and `ReflectionPage` (eyebrow/title/intro/footnote/buttons) under `frontend/src/` (#45)
- [x] T026 [US1] Render the reflection with localized group titles + insight text and a locale-aware fetch of the submit result in `frontend/src/pages/ReflectionPage/` (#46)

**Checkpoint**: a complete bilingual completion journey works end-to-end (MVP). Deployable.

---

## Phase 4: User Story 2 - Switch language mid-flow without losing progress (Priority: P2)

**Goal**: Changing language at any point keeps all selected answers and the current position; only display text changes.

**Independent Test**: Answer a few questions in English, switch to German mid-flow → selections still selected, same position, remaining content in German.

### Tests for User Story 2 (write first, must FAIL)

- [x] T027 [P] [US2] Frontend test: select answers in `en`, switch to `de` mid-flow → selected option IDs preserved, question position unchanged, content now German in `frontend/tests/components/LanguageSwitchMidFlow.test.tsx` (#47)

### Implementation for User Story 2

- [x] T028 [US2] On locale change, re-fetch the questionnaire with the new locale and re-render while preserving in-progress answer selections (keyed by stable option IDs) and the current step in `frontend/src/App.tsx` (the questionnaire flow lives in `App.tsx`, not a `QuestionnairePage/`; behavior landed with US1's locale-aware fetch effect, now locked in by T027) (#48)
- [x] T029 [US2] `LanguageSwitcher` reflects the active locale (`aria-pressed`) and announces the change to assistive tech via a polite live region, with no progress reset, in `frontend/src/components/LanguageSwitcher/LanguageSwitcher.tsx` (#49)

**Checkpoint**: US1 and US2 both work independently.

---

## Phase 5: User Story 3 - See a short title on every reflection insight, in my language (Priority: P2)

**Goal**: Wire the net-new dimension titles end-to-end (both languages) so each insight shows a non-empty title above its dots and text. No reflection-page layout change.

**Independent Test**: Complete the questionnaire; every insight card shows a non-empty title in the active language, in both en and de, with unchanged layout.

### Tests for User Story 3 (write first, must FAIL)

- [x] T030 [P] [US3] Unit test: `ReflectionAssembler` populates `InsightDto.Title` (localized, English fallback) in `backend/tests/CommonGround.UnitTests/` (#50)
- [x] T031 [P] [US3] Integration test: reflection in `en` and `de` has a non-empty title on every insight (SC-004) in `backend/tests/CommonGround.IntegrationTests/` (#51)
- [x] T032 [P] [US3] Frontend test: `InsightCard` shows a non-empty title in the active language with layout unchanged (title → dots → text) in `frontend/tests/components/` (#52)

### Implementation for User Story 3

- [x] T033 [US3] Add `Title` to `InsightDto` in `backend/src/CommonGround.SharedKernel/Interfaces/IReportingService.cs` (#53)
- [x] T034 [US3] `ReflectionAssembler` joins `DimensionTitle` by locale (English fallback) and populates `InsightDto.Title` in `backend/src/CommonGround.Modules.Reporting/ReflectionAssembler.cs` (#54)
- [x] T035 [US3] Seed `DimensionTitle` rows — EN from `reflection-groups.json` `dimensionTitles`, DE from authored titles, via `LocalizationSeedHelper.SeedDimensionTitlesUp` in the hand-authored `20260616140000_SeedDimensionTitles` migration in `backend/src/CommonGround.Api/Persistence/Migrations/` (#55)
- [x] T036 [US3] Confirm `InsightCard` renders the now-populated title (no layout/CSS change) and the title flows from the API through `frontend/src/types/api.ts` usage in `frontend/src/pages/ReflectionPage/` (#56)

**Checkpoint**: every insight has a localized title; comparison feature's row-label dependency is satisfied.

---

## Phase 6: User Story 4 - Open a saved reflection in either language (Priority: P3)

**Goal**: A saved `/me#TOKEN` reflection renders in the viewer's current locale and is switchable at view time; no locale is stored on the response.

**Independent Test**: Complete in German, open the saved link in a fresh session, switch to English → same insights/strengths re-rendered in English.

### Tests for User Story 4 (write first, must FAIL)

- [x] T037 [P] [US4] Integration test: `GET /api/me/reflection?locale=` returns the same insights/strengths re-rendered in the requested locale (en↔de) in `backend/tests/CommonGround.IntegrationTests/` (#57)
- [x] T038 [P] [US4] Frontend test: opening a saved reflection then switching language re-fetches and re-renders in `frontend/tests/components/` (#58)

### Implementation for User Story 4

- [x] T039 [US4] `MeController` accepts `?locale=` and passes it to the assembler in `backend/src/CommonGround.Api/Controllers/MeController.cs` (#59)
- [x] T040 [US4] Mount `LanguageSwitcher` on the saved-reflection page and re-fetch `/api/me/reflection?locale=` on switch in `frontend/src/pages/ReflectionPage/MeReflection.tsx` (#60)

**Checkpoint**: all four stories independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [x] T041 [P] Playwright E2E: full German completion flow (consent → questions → reflection) in `frontend/tests/e2e/` (#61)
- [x] T042 [P] Playwright E2E: mid-flow language switch preserves answers, and `/me` language switch in `frontend/tests/e2e/` (#62)
- [x] T043 [P] Accessibility pass: `<html lang>` correct per locale; switcher keyboard + screen-reader operable; longer German strings do not overflow option/insight cards (WCAG 2.1 AA) across `frontend/src/` (#63)
- [ ] T044 [P] Run [quickstart.md](quickstart.md) validation against SC-001…SC-007 (#64)
- [x] T045 Finalize production DE seed once dimension titles (76), group titles (10), and insight texts (76) are authored and reviewed for neutrality (Principle II); review DE consent copy against consent-UX rules / no double negatives (Principle V); replace placeholders in `AddLocalizationTranslations` (#65)
- [x] T046 [P] Update README/docs for bilingual support and confirm the CI quality gate (coverage ≥ 80% on new code, SonarCloud) is green (#66)

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → no dependencies.
- **Foundational (Phase 2)** → depends on Setup; **blocks all user stories**. Within it: entities (T003–T004) → configs (T005) → migration (T006); T008→T009; T010→T011; T013→T014.
- **User stories (Phases 3–6)** → all depend on Foundational. Recommended order P1 → P2 → P3 → P4.
  - US3 (T033/T034) edits the same `InsightDto`/assembler touched by US1 (T021) — do US3 after US1.
  - US2 builds on US1's questionnaire fetch; US4 reuses US1's assembler signature (adds `MeController`).
- **Polish (Phase 7)** → after the desired stories; T045 also gated by German content authoring.

## Parallel Opportunities

- Setup: T002 ∥ T001.
- Foundational: T003 ∥ T004 ∥ T005, and T008/T010/T012/T013 are independent; frontend (T010–T015) ∥ backend (T003–T009).
- Within a story, the `[P]` test tasks run together; backend (T019–T023) and frontend (T024–T026) of US1 are largely parallel across the API boundary.
- Stories themselves can be parallelized across people once Foundational is done.

## Implementation Strategy

- **MVP = Phases 1, 2, 3 (US1)** — a complete bilingual journey; stop and validate, deploy/demo.
- Then layer US2 (mid-flow switch) → US3 (titles, also unblocks comparison) → US4 (/me switch), each an independent, testable increment.

## Notes

- **Content dependency**: German for dimension titles, group titles, and insight texts is being authored. Code and tests use fixtures (T002); the production seed is finalized in T045. German questions/options are already in `questionary_german.md`.
- **Mutation testing**: Stryker.NET remains blocked on .NET 10 — cover locale resolution, the assembler's localized output, and locale-invariant scoring with unit + integration tests instead.
- **Constitution touchpoints**: deterministic engine unchanged (scoring on IDs/weights); no locale stored server-side (privacy); titles must stay descriptive, not verdicts (neutral outputs); switcher + `lang` meet WCAG 2.1 AA.
