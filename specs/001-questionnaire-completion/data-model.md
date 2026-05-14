# Data Model: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Date**: 2026-05-14

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
Ordered questions within a questionnaire version.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| Text | string | Question text shown to user |
| Dimension | string | e.g. "FeedbackRhythm" |
| OrderIndex | int | Display order |

### AnswerOption
Selectable answers for a question.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionId | Guid | FK → Question |
| Text | string | Answer text shown to user |
| OrderIndex | int | Display order |
| ScoringValue | int | 1–4, used by scoring engine |

### ScoringRule
Describes what each dimension measures. Used for explainability.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| Dimension | string | e.g. "FeedbackRhythm" |
| Description | string | Plain-language description of the dimension |

### InsightTemplate
Predefined neutral insight texts. Looked up by dimension + score range.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| QuestionnaireVersionId | Guid | FK → QuestionnaireVersion |
| Dimension | string | e.g. "FeedbackRhythm" |
| InsightType | enum | PersonalReflection, Alignment, Difference |
| MinScore | int | Inclusive lower bound for score range match |
| MaxScore | int | Inclusive upper bound for score range match |
| Text | string | Neutral, observation-framed insight text |

---

## Module: Responses

### ResponseSet
One participant's completed answers to one questionnaire version.
Immutable once submitted. Soft-deleted (IsDeleted) to preserve audit trail.

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
One participant's answer to one question. Never updated after submission.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ResponseSetId | Guid | FK → ResponseSet |
| QuestionId | Guid | FK → Question |
| AnswerOptionId | Guid | FK → AnswerOption |

**Index**: `(ResponseSetId, QuestionId)` — unique, ensures one answer per question per response.

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
  ├── ScoringRule (1:many)
  ├── InsightTemplate (1:many)
  ├── ResponseSet (1:many)
  │     └── Answer (1:many)
  └── ComparisonSession (1:many)
        └── ComparisonParticipant (1:many)
              └── ResponseSet (many:1)

AuditEvent → ResponseSet (optional)
AuditEvent → ComparisonSession (optional)
```

---

## Scoring Engine Logic

```
For each Dimension in QuestionnaireVersion:
  1. Find all Questions with that Dimension
  2. For each Question, find the Answer submitted by the participant
  3. Get ScoringValue from the AnswerOption
  4. Sum ScoringValues → DimensionScore

For each DimensionScore:
  5. Find InsightTemplate where:
     Dimension = dimension AND
     InsightType = PersonalReflection AND
     MinScore <= DimensionScore <= MaxScore
  6. Add InsightTemplate.Text to personal reflection
```

Score range per dimension (2 questions × max 4 per question): 2–8
- Low: 2–4
- Mid: 5–6
- High: 7–8

---

## Seed Data (MVP Questionnaire v1.0)

**6 dimensions, 2 questions each, 4 answer options each = 12 questions, 48 answer options**

### Dimensions and questions

**FeedbackRhythm**
- Q1: How do you prefer to receive feedback on your work?
- Q2: How often do you like to check in with teammates about ongoing work?

**DecisionMaking**
- Q3: When facing an important decision, what do you prefer?
- Q4: How do you feel about making decisions without full consensus?

**CommunicationStyle**
- Q5: How do you prefer to share updates and progress?
- Q6: When working through a complex problem with a colleague, what works best?

**DeadlineApproach**
- Q7: How do you typically approach deadlines?
- Q8: How do you handle scope changes close to a deadline?

**ConflictHandling**
- Q9: When you disagree with a colleague's approach, what do you tend to do?
- Q10: How do you prefer to resolve recurring friction in a working relationship?

**CollaborationDepth**
- Q11: How do you do your best work?
- Q12: How do you feel about pairing or working closely with a colleague on a task?

*Full question text, answer options with scoring values, and insight templates
will be defined in the seed migration.*
