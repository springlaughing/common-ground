# Implementation Plan: Bilingual (English/German) Support

**Branch**: `002-bilingual-support` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-bilingual-support/spec.md`

## Summary

Make the entire app presentable in English or German, switchable from an accessible
in-UI control, without a second deployment and without touching the deterministic
scoring engine. Display text becomes localized; the stable dimension/option/question
**IDs and scoring weights stay language-agnostic**. Localized content (questions,
answer options, dimension-group titles, the **net-new dimension titles**, and insight
texts) is served per requested locale (`?locale=en|de`, default `en`, field-level
fallback to English). UI chrome is localized client-side. The user's language is held
client-side (accountless); no locale is stored on a `ResponseSet`, so a saved
reflection at `/me#TOKEN` is rendered in the viewer's current language and is
switchable at view time. German content is seeded via an EF Core migration that Render
applies automatically on deploy.

This feature also wires **dimension titles end-to-end for the first time** (they exist
today only in a design artifact and frontend demo data): a new localized title source →
`InsightDto.Title` → API → the existing, currently-empty `<h3>` slot in `InsightCard`.
No reflection-page layout change.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (backend), TypeScript 5 / React 19 (frontend)

**Primary Dependencies**: ASP.NET Core 10, EF Core 10 + Npgsql, xUnit + FluentAssertions
+ Moq, Testcontainers.NET, NetArchTest (backend); React 19, Vite, React Testing Library,
Playwright (frontend). **No new runtime dependency** — frontend i18n is a small in-house
typed message-catalog context (react-i18next considered and rejected, see research.md).

**Storage**: PostgreSQL 16 (Neon, Frankfurt in prod; Testcontainers for integration tests).
New translation tables alongside existing entities. No data migration of existing rows.

**Testing**: xUnit + FluentAssertions + Moq (unit), Testcontainers.NET (integration),
NetArchTest (architecture), React Testing Library (component), Playwright (E2E for the
German completion flow + mid-flow switch). Stryker.NET remains blocked on .NET 10
(unchanged); locale logic is covered by unit/integration tests instead.

**Target Platform**: Web service on Linux container (backend), modern browser (frontend).
Same-origin SPA-in-API container; Render Blueprint `common-ground`, `RunMigrationsOnStartup=true`.

**Project Type**: Web application — ASP.NET Core REST API + React SPA (unchanged from 001).

**Performance Goals**: Switching language and seeing translated content < ~1s perceived
(SC-007). Localized content reads are indexed lookups by `(EntityId, Locale)` — negligible cost.

**Constraints**: Locale affects display text only — never scoring, insight selection, or
ordering (SC-003). WCAG 2.1 AA: switcher operable by mouse/keyboard/screen reader,
`document.documentElement.lang` updated on switch. No locale persisted server-side
(privacy). German strings run longer — layouts must absorb without overflow/truncation.

**Scale/Scope**: Two locales (en, de), extensible to more by adding rows only. ~45
questions, ~180 options, 10 group titles, 76 dimension titles, 76 insight texts, plus a
few dozen UI chrome strings.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Gate | Status | Realized by |
|-----------|------|--------|-------------|
| I. Privacy-First | No new PII; locale held client-side only; no locale on `ResponseSet`; audit unchanged; no raw answers in logs | ✅ Pass | Client-side preference; `?locale` query only |
| II. Neutral Outputs | Dimension titles are descriptive labels, not verdicts; strength dots unchanged; no overall score introduced | ✅ Pass | Title content guidance in [data-model.md](data-model.md); German content reviewed for neutrality |
| III. Deterministic Engine | Locale affects display text only; same version+answers ⇒ same insights/order/strength in any language (SC-003); scoring reads IDs/weights, never text | ✅ Pass | `ScoringEngine` untouched; `ReflectionAssembler` localizes text post-scoring |
| IV. Modular Monolith | Question/AnswerOption translations owned by Questionnaires; InsightSnippet/DimensionGroup/DimensionTitle translations owned by Reporting; cross-module via SharedKernel interfaces; arch tests extended | ✅ Pass | New entities placed in owning modules; NetArchTest cases added |
| V. Explicit Consent | Consent step localized in both languages; accept/decline keep equal visual weight; no semantics change; no double negatives in DE | ✅ Pass | Localized `ConsentStep` chrome; DE consent copy reviewed against consent-UX rules |
| VI. Accountless Identity | No accounts; `/me#TOKEN` fragment flow unchanged; locale is a view-time query param, not identity | ✅ Pass | `GET /api/me/reflection?locale=` |
| VII. DevSecOps | Tests precede impl (TDD); CI quality gate; ADR for the localization schema decision; PR linked to issues from tasks | ✅ Pass | [ADR-0008](../../docs/adr/) (to be written); coverage on new code ≥ 80% |

**Version immutability (Development Standards → Data model)**: "Questionnaire versions
are immutable once responses exist." Localization is **additive display metadata** keyed
to the same immutable question/option IDs and weights — it does not change any question's
identity, its options, or its scoring. Existing responses and their computed
`DimensionScore`s remain valid and unchanged. Therefore **no new questionnaire version is
required**, and this is not a violation. (Recorded in Complexity Tracking for visibility.)

No unjustified violations. Architecture decisions indexed in [`docs/adr/`](../../docs/adr/README.md).

## Project Structure

### Documentation (this feature)

```text
specs/002-bilingual-support/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── api.md           ← Phase 1 output (locale params + InsightDto.title)
├── checklists/
│   └── requirements.md  ← spec quality checklist (from /speckit-specify)
└── tasks.md             ← Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── CommonGround.Modules.Questionnaires/
│   │   ├── Entities/QuestionTranslation.cs          # NEW (QuestionId, Locale, Text)
│   │   ├── Entities/AnswerOptionTranslation.cs      # NEW (AnswerOptionId, Locale, Text)
│   │   └── (reader returns localized text + en fallback)
│   ├── CommonGround.Modules.Reporting/
│   │   ├── Entities/InsightSnippetTranslation.cs    # NEW (InsightSnippetId, Locale, Text)
│   │   ├── Entities/DimensionGroupTranslation.cs    # NEW (DimensionGroupId, Locale, Title)
│   │   ├── Entities/DimensionTitle.cs               # NEW, locale-first (DimensionId, Locale, Title)
│   │   └── ReflectionAssembler.cs                   # CHANGED: locale param, join titles, en fallback
│   ├── CommonGround.SharedKernel/Interfaces/
│   │   └── IReportingService.cs                     # CHANGED: InsightDto gains Title; assemble(locale)
│   └── CommonGround.Api/
│       ├── Controllers/QuestionnaireController.cs   # CHANGED: ?locale=
│       ├── Controllers/ResponsesController.cs       # CHANGED: ?locale= on returned reflection
│       ├── Controllers/MeController.cs              # CHANGED: ?locale= (switch at view time)
│       ├── Localization/SupportedLocales.cs         # NEW: {en, de}, default en, normalise/validate
│       └── Persistence/
│           ├── Configurations/*TranslationConfiguration.cs  # NEW EF configs + unique (EntityId,Locale)
│           └── Migrations/<ts>_AddLocalizationTranslations.* # NEW migration + DE/EN seed
└── tests/
    ├── CommonGround.UnitTests/                      # locale resolution+fallback; assembler title/text; scoring locale-invariant
    ├── CommonGround.IntegrationTests/               # ?locale=de end-to-end; fallback; migration applies
    └── CommonGround.ArchitectureTests/              # translation entities respect module boundaries

frontend/
├── src/
│   ├── i18n/
│   │   ├── LanguageContext.tsx                      # NEW: current locale, localStorage, sets <html lang>
│   │   ├── messages.en.ts / messages.de.ts         # NEW: typed UI-chrome catalogs
│   │   └── useMessages.ts                           # NEW: typed lookup hook
│   ├── components/
│   │   ├── LanguageSwitcher/LanguageSwitcher.tsx    # NEW: accessible EN/DE control (on every primary page)
│   │   ├── ConsentStep/ · ProgressIndicator/ · CredentialsDisplay/ · QuestionStep/  # CHANGED: chrome via useMessages
│   │   └── InsightCard/                             # UNCHANGED (already has title slot)
│   ├── pages/
│   │   ├── QuestionnairePage/ · ReflectionPage/ · ReflectionPage/MeReflection.tsx   # CHANGED: switcher + locale-aware fetch
│   └── services/questionnaireApi.ts                # CHANGED: send ?locale=<current> on content calls
└── tests/
    ├── components/                                  # LanguageSwitcher, localized chrome, InsightCard shows title, switch preserves answers
    └── e2e/                                         # German full flow; mid-flow switch keeps answers; /me switch
```

**Structure Decision**: Unchanged web-app layout from 001. New translation entities live
in their owning modules (Questionnaires, Reporting) to preserve enforced boundaries.
Frontend gains a small `i18n/` folder; no new runtime dependency.

## Complexity Tracking

| Note | Why | Why the simpler alternative was not taken |
|------|-----|-------------------------------------------|
| Translations added to existing immutable v1.0 (no new version) | Localization is display-only metadata on the same IDs/weights; existing scores unaffected | Cutting a new questionnaire version would force re-answering and break existing saved reflections for zero scoring benefit |
| Hybrid schema: existing entities keep English in their base column + translation table for other locales; net-new dimension titles are locale-first | Avoids moving/rewriting existing seeded English rows and directly implements field-level English fallback (FR-013) | Moving all text into uniform translation tables (per the original memo) is cleaner on paper but a higher-risk migration on a deployed DB for no user-visible gain now (see research.md) |
