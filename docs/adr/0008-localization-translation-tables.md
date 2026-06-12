# ADR 0008: Localize content via per-locale translation tables with an English base/fallback

## Status

Accepted

## Context

Feature [002-bilingual-support](../../specs/002-bilingual-support/spec.md) makes the app
presentable in English or German from one deployment, switchable via an in-UI control.
Only **display text** is localized — the deterministic scoring engine
([ADR-0002](0002-use-deterministic-comparison-engine.md)) operates on stable dimension
IDs and weights, never on text, and must produce identical insights/order/strength
regardless of language.

The content that needs localizing already exists as **English seeded directly in base
columns** (`Question.Text`, `AnswerOption.Text`, `InsightSnippet.Text`,
`DimensionGroup.Title`) via the `SeedDataHelper` migration. One category is **net-new**:
the short per-dimension titles shown on each reflection insight exist today only in the
`reflection-groups.json` design artifact and in frontend demo data — they are not wired
through the backend at all.

Constraints shaping the decision:

- **Version immutability** — "questionnaire versions are immutable once responses exist."
  The live (Neon) DB already holds responses against v1.0.
- **Privacy-First** ([ADR-0003](0003-use-private-links-and-access-codes.md)) — minimize
  stored personal data; the accountless model holds language client-side.
- **Extensibility** — adding a third language later should require data only, not schema
  or scoring changes.
- **Partial content** — German for questions/options is ready; titles, group titles, and
  insight texts are still being authored, so the design must not block code on copy.

## Decision

**Hybrid translation tables.**

1. **Existing textual entities keep their current column as the canonical English /
   fallback.** A translation table per entity holds rows for **non-default locales** only
   (German now), keyed `(EntityId, Locale)` with a unique index:
   `QuestionTranslation`, `AnswerOptionTranslation`, `InsightSnippetTranslation`,
   `DimensionGroupTranslation`.

2. **The net-new dimension titles are locale-first.** A `DimensionTitle` table holds one
   row per `(DimensionId, Locale)` for **both** `en` and `de` — no base column. English is
   the fallback locale.

3. **Transport: an explicit `?locale=en|de` query parameter** on the content endpoints
   (`GET /api/questionnaire/current`, `POST /api/responses`, `GET /api/me/reflection`).
   Default `en`; an unknown/unsupported value resolves to `en`.

4. **Field-level English fallback.** If a requested-locale translation row is missing for
   an item, serve the English base value for that item; the rest of the page stays in the
   requested locale.

5. **No locale is stored on `ResponseSet`.** Language lives client-side. A saved
   reflection (`/me#TOKEN`) is rendered in the viewer's currently selected locale and is
   switchable at view time — the same reflection can be read in either language.

6. **Localization is additive to the immutable v1.0.** Translations are display metadata
   keyed to the same question/option IDs and weights; no scoring definition changes, and
   existing responses and their `DimensionScore`s remain valid. **No new questionnaire
   version is cut.**

UI chrome (consent, headings, progress, buttons) is localized **client-side** via typed
message catalogs, not the database.

## Consequences

### Positive

- **No data migration of existing rows.** English stays where it is; we only add tables
  and German rows — lowest risk on the live DB.
- **Field-level fallback falls out for free** — the base column *is* the English fallback,
  satisfying FR-013 without extra machinery, and tolerating partially-authored German.
- **Scoring is provably untouched** — locale resolves text after selection/ordering, so
  identical answers yield identical insights/strengths in any language (SC-003).
- **Privacy-minimal** — nothing about language is persisted against a response.
- **Extensible** — a third language is rows only: insert translations + add a chrome
  catalog + extend the supported-locale set (FR-015).

### Negative / trade-offs

- **Schema asymmetry.** Existing entities use base-column-plus-translation-table while
  `DimensionTitle` is locale-first. This is deliberate (net-new content gets the clean
  model; existing content avoids a risky move) but means two patterns coexist.
- **English is "special"** as the base/fallback locale rather than just another row. If a
  future requirement makes English non-privileged, the existing-entity text would need to
  migrate into uniform translation tables (see Alternatives) — a follow-up, not a blocker.
- **Per-field fallback** can produce a mixed-language page if German is incomplete. This is
  intended robustness during authoring; T045 finalizes the German seed before release.

## Alternatives considered

### Uniform translation tables — move ALL text (including English) out of base columns

Conceptually cleaner (every locale is just a row; no privileged base). **Rejected for
now**: it requires moving existing English rows and emptying/dropping base columns on a
deployed DB, plus rewriting the existing seed — higher risk for no user-visible benefit.
The door is left open to migrate later.

### JSON blob of locales per row

Rejected — not indexable/queryable per locale, weak typing, harder to seed and validate.

### `Accept-Language` header negotiation instead of `?locale=`

Rejected — implicit and harder to override from a user-chosen switcher; the explicit query
parameter is cache-friendly and trivially testable, and matches a *user-selected* (not
browser-detected) language.

### Store the completion-time locale on `ResponseSet` and lock the saved view to it

Rejected — adds a stored attribute against the response for no benefit. Rendering the
saved reflection in the viewer's current locale is more flexible and keeps the response
free of language data. (One nullable column would reverse this if ever needed.)

### Locale in the URL path (`/de/...`)

Rejected — heavier routing change than a 2-locale, client-preference MVP warrants.

## Related

- [ADR-0002](0002-use-deterministic-comparison-engine.md) — the deterministic engine this
  decision must not disturb.
- [ADR-0003](0003-use-private-links-and-access-codes.md) — the accountless/privacy model
  that keeps language client-side.
- Realizes spec requirements FR-003, FR-007, FR-008, FR-011, FR-013, FR-015 in
  [`specs/002-bilingual-support`](../../specs/002-bilingual-support/spec.md); design in
  [`research.md`](../../specs/002-bilingual-support/research.md) (R1–R6) and
  [`data-model.md`](../../specs/002-bilingual-support/data-model.md).
- Implemented by tasks T003–T006 (schema), T021/T034 (assembler), T035 (titles seed),
  T039 (`/me` locale) in `specs/002-bilingual-support/tasks.md`.
