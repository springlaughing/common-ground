# Data Model: Bilingual (English/German) Support

**Feature**: 002-bilingual-support | **Date**: 2026-06-12

Extends the [001 data model](../001-questionnaire-completion/data-model.md). Only
**additions and changes** are listed here. All new entities use `Guid` PKs and follow the
existing module ownership and configuration conventions.

`Locale` is a short string code: `"en"` (default/fallback) or `"de"`. Designed to admit
more codes later without schema change.

---

## New: localized text tables

### Module: Questionnaires

#### QuestionTranslation
German (and future non-default) text for a question. English stays in `Question.Text`.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionId | Guid | FK → Question |
| Locale | string | e.g. "de" |
| Text | string | Localized question text |

**Index**: `(QuestionId, Locale)` — unique.

#### AnswerOptionTranslation
Localized answer-option text. English stays in `AnswerOption.Text`. **Option IDs are
unchanged across locales** — this is what lets a mid-flow language switch preserve selected
answers (selections reference the stable `AnswerOption.Id`, not its text).

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AnswerOptionId | Guid | FK → AnswerOption |
| Locale | string | e.g. "de" |
| Text | string | Localized option text |

**Index**: `(AnswerOptionId, Locale)` — unique.

### Module: Reporting

#### InsightSnippetTranslation
Localized second-person insight text. English stays in `InsightSnippet.Text`.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| InsightSnippetId | Guid | FK → InsightSnippet |
| Locale | string | e.g. "de" |
| Text | string | Localized insight text |

**Index**: `(InsightSnippetId, Locale)` — unique.

#### DimensionGroupTranslation
Localized dimension-group title. English stays in `DimensionGroup.Title`.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| DimensionGroupId | Guid | FK → DimensionGroup |
| Locale | string | e.g. "de" |
| Title | string | Localized group title |

**Index**: `(DimensionGroupId, Locale)` — unique.

#### DimensionTitle  *(NET-NEW content — locale-first)*
Short per-dimension title shown on each reflection insight (above the strength dots). Did
not exist end-to-end before this feature. **Locale-first**: rows for both `en` and `de`;
no base column. English is the fallback locale.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| DimensionId | string | References a dimension id (same string keys used elsewhere) |
| Locale | string | "en" or "de" |
| Title | string | Short, neutral, descriptive label (e.g. "Written records over memory") |

**Index**: `(DimensionId, Locale)` — unique.

**Content guidance (Principle II — Neutral Outputs)**: titles MUST be descriptive labels,
never verdicts or ratings. English source = `reflection-groups.json` `dimensionTitles`.

---

## Changed: reflection DTOs (SharedKernel)

`InsightDto` gains a localized `Title`; the assembler takes a locale.

```csharp
// IReportingService
Task<ReflectionDto> AssembleReflectionAsync(Guid responseSetId, string locale, CancellationToken ct = default);

public record ReflectionGroupDto(string Id, string Title /* localized */, IReadOnlyList<InsightDto> Insights);

public record InsightDto(
    string DimensionId,
    string Title,   // NEW — localized dimension title (en fallback)
    string Text,    // localized insight text (en fallback)
    int Strength);  // unchanged — derived from score, locale-invariant
```

The frontend `InsightDto` type already declares `title: string`; it is currently never
populated. After this change the API supplies it, filling the existing `<h3>` in `InsightCard`.

---

## Localized read logic (per locale, with English fallback)

For a requested `locale`:

```
text(entity)      = translation[entity, locale]?.Text  ?? entity.BaseText      // EN fallback
title(group)      = groupTranslation[group, locale]?.Title ?? group.Title      // EN fallback
title(dimension)  = dimensionTitle[dim, locale]?.Title
                    ?? dimensionTitle[dim, "en"].Title                          // EN fallback (locale-first table)
```

**Invariant (Principle III)**: which insights appear, their group, order, and `Strength`
are computed by the unchanged `ScoringEngine` / `ReflectionAssembler` selection logic from
`DimensionScore` (IDs + weights only). `locale` changes **only** the resolved strings.
⇒ identical structure/strength across locales for identical answers (SC-003).

---

## Seed (new EF Core migration `AddLocalizationTranslations`)

Inserts, deterministically (same hashing/ID conventions as `SeedDataHelper`):

- `QuestionTranslation` (de) — from `questionary_german.md`
- `AnswerOptionTranslation` (de) — from `questionary_german.md`
- `DimensionGroupTranslation` (de) — from authored DE group titles
- `InsightSnippetTranslation` (de) — from authored DE insight texts
- `DimensionTitle` (en) — from `reflection-groups.json` `dimensionTitles`
- `DimensionTitle` (de) — from authored DE dimension titles

`SeedDown` deletes the inserted translation/title rows only. Render applies the migration
on deploy (`RunMigrationsOnStartup=true`). DE content for titles/groups/insights is a
content dependency (research.md R7); code/tests use fixtures until it is finalized.

---

## Relationships (additions)

```
Question        ──1:many── QuestionTranslation        (locale rows)
AnswerOption    ──1:many── AnswerOptionTranslation    (locale rows)
InsightSnippet  ──1:many── InsightSnippetTranslation  (locale rows)
DimensionGroup  ──1:many── DimensionGroupTranslation  (locale rows)
DimensionTitle  (keyed by DimensionId string + Locale; en + de rows)   // net-new
```

No change to `ResponseSet`, `Answer`, `DimensionScore`, `DimensionWeight`,
`DimensionMaxScore`, `AuditEvent`, or the Comparisons tables. **No locale is stored on any
response-side entity** (privacy; R4).
