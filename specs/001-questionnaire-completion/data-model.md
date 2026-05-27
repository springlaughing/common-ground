# Data Model: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Date**: 2026-05-14 (updated 2026-05-27)

All entities use `Guid` primary keys. All timestamps are `DateTimeOffset` (UTC).
Module ownership is noted — cross-module access goes through SharedKernel interfaces only.

---

## Module: Questionnaires

### QuestionnaireVersion
Immutable once responses exist. Only one version is active at a time in MVP.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| VersionNumber | string | e.g. "1.0" |
| IsActive | bool | Only one active version at a time |
| CreatedAt | DateTimeOffset | |

### Question
Ordered questions within a questionnaire version. Questions belong to a section
for UI grouping and progress display.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| Text | string | Question text shown to user |
| SectionIndex | int | Which of the 10 sections this question belongs to (1–10) |
| OrderIndex | int | Global display order across all sections |

### AnswerOption
Selectable answers for a question. Display only — scoring weights live in `DimensionWeight`.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionId | Guid | FK → Question |
| Text | string | Answer text shown to user |
| OrderIndex | int | Display order within the question (A/B/C/D) |

### DimensionWeight
Dimension scoring weights per answer option. Each answer option may carry weights
on multiple dimensions. Weights may be positive or negative.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AnswerOptionId | Guid | FK → AnswerOption |
| DimensionId | string | References a dimension id from `dimensions.json` |
| Weight | int | Contribution to this dimension when this option is selected (e.g. +2, +1, -1) |

**Index**: `(AnswerOptionId, DimensionId)` — unique per option per dimension.

### DimensionMaxScore
Maximum achievable raw score per dimension for a questionnaire version.
Computed at seed time — used to normalise raw scores to 0.0–1.0.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| DimensionId | string | References a dimension id from `dimensions.json` |
| MaxScore | decimal | Max raw score achievable: sum of (best primary weight × 1.0) + (best secondary weight × 0.5) across all questions |

**Index**: `(QuestionnaireVersionId, DimensionId)` — unique.

---

## Module: Responses

### ResponseSet
One participant's completed answers to one questionnaire version.
Immutable once submitted. Soft-deleted (`IsDeleted`) to preserve audit trail.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| PrivateResultTokenHash | string | HMAC-SHA256 hash of the private result token |
| AccessCodeHash | string | HMAC-SHA256 hash of the access code |
| CreatedAt | DateTimeOffset | |
| IsDeleted | bool | Soft delete — disables access, nulls answers |
| DeletedAt | DateTimeOffset? | Nullable |

### Answer
One participant's answer to one question. Stores both primary and optional secondary
selection. Never updated after submission.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ResponseSetId | Guid | FK → ResponseSet |
| QuestionId | Guid | FK → Question |
| PrimaryAnswerOptionId | Guid | FK → AnswerOption — applied at weight ×1.0 |
| SecondaryAnswerOptionId | Guid? | Nullable FK → AnswerOption — applied at weight ×0.5 if present |

**Index**: `(ResponseSetId, QuestionId)` — unique, ensures one answer per question per response.

---

## Module: Reporting

### DimensionScore
Computed dimension scores for a response set. Written once after submission,
never updated. Used to render the personal reflection and future comparison reports.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ResponseSetId | Guid | FK → ResponseSet |
| DimensionId | string | References a dimension id from `dimensions.json` |
| RawScore | decimal | Sum of weighted contributions from primary (×1.0) and secondary (×0.5) answers |
| NormalisedScore | decimal | RawScore / DimensionMaxScore — range 0.0–1.0 |

**Index**: `(ResponseSetId, DimensionId)` — unique.

### InsightSnippet
Pre-authored second-person insight texts, seeded from `reflection-groups.json`.
One row per dimension. Not versioned per questionnaire version in MVP.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| DimensionId | string | References a dimension id from `dimensions.json` |
| Text | string | User-facing insight text in second-person voice |

### DimensionGroup
Named groups that organise dimensions on the personal reflection page.
Seeded from `reflection-groups.json`. Not versioned per questionnaire version in MVP.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| GroupId | string | e.g. "work_context_expectations_and_alignment" |
| Title | string | Human-readable group title shown on reflection page |
| OrderIndex | int | Display order of the group |

### DimensionGroupMembership
Maps dimensions to their group. Seeded from `reflection-groups.json`.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| DimensionGroupId | Guid | FK → DimensionGroup |
| DimensionId | string | References a dimension id from `dimensions.json` |
| OrderIndex | int | Display order within the group |

---

## Module: Comparisons
*(Tables created now to satisfy the constitution's group-comparison-from-day-one requirement.
Not populated by this feature — populated in spec 002.)*

### ComparisonSession
A comparison between two or more participants. Supports group comparisons by design.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| Status | enum | Pending, Complete, Unavailable |
| CreatedAt | DateTimeOffset | |

### ComparisonParticipant
Links a ResponseSet to a ComparisonSession. Multiple rows per session = group support.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ComparisonSessionId | Guid | FK → ComparisonSession |
| ResponseSetId | Guid | FK → ResponseSet |
| Role | enum | Initiator, Invitee |
| JoinedAt | DateTimeOffset | |

---

## Module: Audit

### AuditEvent
Append-only. Never contains raw answers, tokens, or access codes.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| EventType | string | e.g. "questionnaire_started", "questionnaire_completed", "personal_reflection_generated" |
| ResponseSetId | Guid? | Nullable FK — omitted for pre-response events |
| ComparisonSessionId | Guid? | Nullable FK |
| OccurredAt | DateTimeOffset | |
| Metadata | string | JSON — context only, no raw answers/tokens |

---

## Relationships Overview

```
QuestionnaireVersion
  ├── Question (1:many)
  │     └── AnswerOption (1:many)
  │           └── DimensionWeight (1:many)
  ├── DimensionMaxScore (1:many)
  ├── ResponseSet (1:many)
  │     ├── Answer (1:many)
  │     │     ├── AnswerOption (PrimaryAnswerOptionId)
  │     │     └── AnswerOption? (SecondaryAnswerOptionId)
  │     └── DimensionScore (1:many)
  └── ComparisonSession (1:many)
        └── ComparisonParticipant (1:many)
              └── ResponseSet (many:1)

DimensionGroup
  └── DimensionGroupMembership (1:many)

InsightSnippet (keyed by DimensionId string)
DimensionGroupMembership (keyed by DimensionId string)
DimensionWeight (keyed by DimensionId string)
DimensionMaxScore (keyed by DimensionId string)
DimensionScore (keyed by DimensionId string)

AuditEvent → ResponseSet (optional)
AuditEvent → ComparisonSession (optional)
```

---

## Scoring Engine Logic

```
Input: ResponseSet (all Answers with Primary + optional Secondary selections)

Step 1 — Accumulate raw scores:
  For each Answer in ResponseSet:
    For each DimensionWeight on PrimaryAnswerOptionId:
      RawScores[DimensionId] += Weight × 1.0
    If SecondaryAnswerOptionId is set:
      For each DimensionWeight on SecondaryAnswerOptionId:
        RawScores[DimensionId] += Weight × 0.5

Step 2 — Normalise:
  For each DimensionId in RawScores:
    MaxScore = DimensionMaxScore[QuestionnaireVersionId, DimensionId]
    NormalisedScore = RawScores[DimensionId] / MaxScore  → clamped to 0.0–1.0

Step 3 — Persist:
  Write one DimensionScore row per dimension

Step 4 — Build reflection:
  For each DimensionGroup (in OrderIndex order):
    For each DimensionId in group (in OrderIndex order):
      If NormalisedScore >= 0.4 (display threshold):
        Fetch InsightSnippet.Text for DimensionId
        Compute strength = max(1, min(5, ceil(NormalisedScore × 5)))
        Add insight to group output
    If group has no qualifying insights: omit group from output
```

**DimensionMaxScore computation (at seed time)**:
```
For each DimensionId across all AnswerOptions in the questionnaire version:
  For each Question that has at least one AnswerOption with a weight on this DimensionId:
    BestPrimary   = max weight for this DimensionId across all AnswerOptions for this Question
    BestSecondary = second-highest weight (or 0 if only one option has a weight)
    MaxScore += (BestPrimary × 1.0) + (BestSecondary × 0.5)
```

---

## Seed Data

The active questionnaire (v1.0) is seeded from three source files at the root of the repository:

| File | Content |
|------|---------|
| `questionary.md` | All 10 sections, ~60 questions, answer option texts, and hidden dimension weight mappings |
| `dimensions.json` | All 76 dimension definitions with IDs and descriptions |
| `reflection-groups.json` | 10 dimension groups with titles, dimension membership, and 76 insight snippets |

The seed migration reads these files and populates:
- `QuestionnaireVersion` (one active row, v1.0)
- `Question` and `AnswerOption` rows from `questionary.md`
- `DimensionWeight` rows from the hidden mappings in `questionary.md`
- `DimensionMaxScore` rows computed from the weight table
- `InsightSnippet` rows from `reflection-groups.json`
- `DimensionGroup` and `DimensionGroupMembership` rows from `reflection-groups.json`
