# Phase 0 Research: Bilingual (English/German) Support

**Feature**: 002-bilingual-support | **Date**: 2026-06-12

All Technical Context unknowns are resolved below. Each item: Decision · Rationale ·
Alternatives considered.

---

## R1. Where localized text lives (schema approach)

**Decision**: Hybrid. Existing textual entities (`Question.Text`, `AnswerOption.Text`,
`InsightSnippet.Text`, `DimensionGroup.Title`) keep their current column as the
**canonical English / fallback**. Add a translation table per entity holding rows for
**non-default locales** (German now), keyed `(EntityId, Locale)`. The **net-new dimension
titles** have no existing column, so they are **locale-first**: a `DimensionTitle` table
with one row per `(DimensionId, Locale)` for both `en` and `de`.

**Rationale**:
- No data migration of existing seeded rows → lowest risk on the live Neon DB.
- The base column *is* the English fallback, so FR-013 (field-level fallback to English)
  falls out naturally: read translation for the requested locale; if absent, use the base column.
- Dimension titles are new, so they get the clean uniform treatment from the start.
- Adding a third language later = inserting rows only; no schema change (FR-015).

**Alternatives considered**:
- *Uniform translation tables (move ALL text out of base columns, including English)* —
  matches the original 10-day-old memo. Cleaner conceptually, but requires moving existing
  English rows and emptying/dropping base columns on a deployed DB, plus rewriting the
  existing seed. Higher risk, no user-visible benefit now. Rejected; door left open to
  migrate later.
- *JSON blob of locales per row* — not queryable/indexable per locale, weak typing, harder
  to seed and validate. Rejected.

---

## R2. How the client requests a locale

**Decision**: Explicit `?locale=en|de` query parameter on the three content endpoints
(`GET /api/questionnaire/current`, `POST /api/responses`, `GET /api/me/reflection`).
Default `en`. Unknown/unsupported value → treated as `en` (no error). A single
`SupportedLocales` helper normalises and validates.

**Rationale**: Explicit, cache-friendly, trivially testable, and mirrors the client-side
language preference (the SPA already knows the chosen locale and just appends it).
Switching language at view time is a re-fetch with a different `?locale`.

**Alternatives considered**:
- *`Accept-Language` header negotiation* — implicit, harder to override from the UI,
  muddier to test, and the SPA would have to set a header rather than a param. Rejected;
  the explicit param better matches a user-chosen (not browser-chosen) language.
- *Locale in the URL path (`/de/...`)* — heavier routing change for a 2-locale MVP with a
  client-side preference. Rejected.

---

## R3. Frontend UI-chrome localization mechanism

**Decision**: Small in-house typed message catalogs (`messages.en.ts`, `messages.de.ts`)
behind a `LanguageContext` + `useMessages()` hook. The context holds the active locale,
persists it to `localStorage`, and sets `document.documentElement.lang`.

**Rationale**: Scope is ~a few dozen static strings in two languages — no pluralization or
runtime formatting needs. TypeScript strict mode gives compile-time guarantees that every
key exists in both catalogs. Keeps the zero-runtime-dependency frontend lean (bundle/audit
surface). The same context drives the API client's `?locale` and the `<html lang>` attribute.

**Alternatives considered**:
- *react-i18next* (named in the old memo) — battle-tested but adds a dependency and config
  weight that the current scope does not justify. Reconsider if/when string count or
  pluralization/interpolation needs grow. Rejected for now.

---

## R4. Saved reflection (`/me#TOKEN`) language behavior

**Decision** (confirmed with the user): Render in the **viewer's currently selected
locale**, switchable at view time. **No locale is stored on `ResponseSet`.**

**Rationale**: Reflection text is a localized lookup keyed by stable dimension IDs;
serving either language is a re-fetch. Storing nothing keeps the model privacy-minimal and
accountless-consistent, and lets the same reflection be read in either language.

**Alternatives considered**:
- *Persist the completion-time locale on `ResponseSet` and lock the view to it* — adds a
  stored attribute against the response for no real benefit and slightly more data.
  Rejected. (Easy to revisit: it would be one nullable column + a default in the reader.)

---

## R5. Reconciling with questionnaire-version immutability

**Decision**: Add translations to the existing active v1.0 — do **not** cut a new version.

**Rationale**: The immutability rule protects the **scoring definition** (questions,
options, weights). Translations are additive display metadata on the same immutable IDs;
no weight, option, dimension, or ordering changes. Existing responses and their
`DimensionScore`s are unaffected and remain reproducible. Cutting a new version would force
re-answering and orphan existing saved reflections for zero scoring benefit.

---

## R6. Missing-translation behavior

**Decision**: Field-level fallback to English. If a requested-locale translation row is
missing for a given item, serve the English base value for that item; the rest of the page
stays in the requested locale. Never render an empty/broken element (FR-013).

**Rationale**: Robust to partial translation during content authoring and to future added
strings. Per-field (not per-page) fallback keeps the experience coherent.

---

## R7. Content authoring status / dependency

**Decision**: Treat German content as a tracked dependency of the seed task, not a code blocker.

**Status**:
- ✅ German **questions + answer options** drafted in `questionary_german.md`.
- ⏳ German **dimension titles (76)**, **dimension-group titles (10)**, and **insight
  texts (76)** still need authoring. English dimension titles already exist as a source in
  `reflection-groups.json` (`dimensionTitles`).
- 🚫 Out of scope: 76 third-person ("They…") insight texts (`reflection-groups-third-person.json`) —
  belong to the future comparison feature.

**Rationale**: The schema, API, and frontend can be built and tested against fixture
translations; the production DE seed lands when the remaining DE copy is finalized and
reviewed for neutrality (Principle II) and consent-UX rules (Principle V).

---

## Resolved unknowns summary

| Unknown | Resolution |
|---------|-----------|
| Localized text storage | Hybrid: base column = EN fallback + per-locale translation tables; titles locale-first (R1) |
| Locale transport | `?locale=en|de`, default `en`, unknown→`en` (R2) |
| Frontend chrome i18n | In-house typed catalogs + context; no new dependency (R3) |
| `/me` language | Viewer's current locale, switchable, none stored (R4) |
| Version immutability | Additive metadata; no new version (R5) |
| Missing translation | Field-level English fallback (R6) |
| DE content readiness | Questions/options done; titles/groups/insights pending; third-person deferred (R7) |
