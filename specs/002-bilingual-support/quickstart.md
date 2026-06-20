# Quickstart: Bilingual (English/German) Support

**Feature**: 002-bilingual-support | **Date**: 2026-06-12

How to run, exercise, and extend bilingual support locally.

## Run locally

Backend + DB (from `backend/`), then frontend (from `frontend/`):

```powershell
# Backend (applies migrations incl. AddLocalizationTranslations against local Postgres)
dotnet run --project src/CommonGround.Api

# Frontend
npm run dev   # http://localhost:5173
```

## Try both languages

- **Frontend**: use the language switcher (present on the consent step, every question,
  and the reflection page). It persists to `localStorage` and updates `<html lang>`.
- **API directly**:
  ```
  GET  /api/questionnaire/current?locale=de
  POST /api/responses?locale=de
  GET  /api/me/reflection?locale=en   # switch a saved reflection's language by changing locale
  ```
- **Reflection design harness** (no backend needed): `http://localhost:5173/?preview=reflection`
  already renders insight titles from demo data; this feature makes the real API supply them.

## What to verify (maps to Success Criteria)

- **SC-001**: Complete the full flow in German — no English text leaks anywhere.
- **SC-002**: Answer a few questions in English, switch to German → answers stay selected,
  same question position. (Selections key off stable option IDs.)
- **SC-003**: Submit the same answers with `?locale=en` vs `?locale=de` → identical
  insights, order, and `strength`; only `title`/`text` differ.
- **SC-004**: Every insight card shows a non-empty title in the active language.
- **SC-005**: Open a saved `/me#TOKEN` reflection, switch language → re-renders, same
  insights/strengths.
- **SC-006**: Switcher reachable + operable by keyboard and screen reader on every primary page.
- **Fallback (FR-013)**: temporarily remove one German translation row → that item shows
  English while the rest of the page stays German; no empty element.

## Add another language later (FR-015)

1. Insert translation rows for the new locale code (questions, options, group titles,
   dimension titles, insight texts) — no schema change.
2. Add a `messages.<locale>.ts` UI-chrome catalog (TypeScript enforces full key coverage).
3. Add the locale to `SupportedLocales` and to the switcher options.

No scoring rule, dimension ID, question structure, or insight-selection change is required.

## Tests

```powershell
# Backend
dotnet test                      # unit, integration (Testcontainers), architecture
# Frontend
npm run test                     # component + context tests
npm run test:e2e                 # German full flow, mid-flow switch, /me switch
```

Key cases: locale resolution + English fallback; assembler returns localized title/text;
**scoring is locale-invariant**; mid-flow switch preserves answers; `InsightCard` renders a
title; `<html lang>` updates on switch.

## Validation (T044, 2026-06-20)

Each success criterion is covered by an automated test (frontend unit 46/46 + E2E 3/3
green locally; backend integration/unit green in CI on PRs #79/#80/#81):

| Criterion | Evidence | Status |
| --- | --- | --- |
| SC-001 — full flow in German, no English leaks | E2E `german-flow.spec.ts`; backend `GetCurrent_LocaleDe_ReturnsGermanText_SameIdsAndOrderAsEnglish` | ✅ |
| SC-002 — switch mid-flow keeps answers + position | E2E `language-switch.spec.ts` (mid-flow) | ✅ |
| SC-003 — locale-invariant scoring (same insights/order/strength) | backend `Submit_LocaleDe_ReturnsGermanReflection_SameInsightsOrderAndStrengthAsEnglish` | ✅ |
| SC-004 — every insight card has a non-empty localized title | backend `Submit_EveryInsight_HasNonEmptyTitle_LocalizedPerLocale`; frontend `InsightCard` test | ✅ |
| SC-005 — `/me` switch re-renders, same insights/strengths | E2E `language-switch.spec.ts` (`/me` switch) | ✅ |
| SC-006 — switcher keyboard + screen-reader operable | accessibility tests (T043): `LanguageSwitcher`, `<html lang>` | ✅ |
| FR-013 — missing translation falls back to English per item | backend `SupportedLocalesTests`, `ReflectionAssemblerTests` (fallback) | ✅ |
