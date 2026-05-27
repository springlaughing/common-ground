# API Contracts: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Date**: 2026-05-14 (updated 2026-05-27)
**Base URL**: `/api`

All requests and responses use JSON. All endpoints require HTTPS.
Timestamps are ISO 8601 UTC strings.

---

## GET /api/questionnaire/current

Returns the active questionnaire version with all questions and answer options,
ordered for rendering.

**Auth**: None required.

**Response 200**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "versionNumber": "1.0",
  "questions": [
    {
      "id": "a1b2c3d4-...",
      "text": "When starting work with someone new, what helps you feel ready to collaborate?",
      "sectionIndex": 1,
      "orderIndex": 1,
      "answerOptions": [
        { "id": "e5f6...", "text": "Written goals, context, and expectations — I need to understand what we are working toward before I can contribute.", "orderIndex": 1 },
        { "id": "g7h8...", "text": "A live conversation to ask questions and calibrate — written context only tells me what someone decided to write down.", "orderIndex": 2 },
        { "id": "i9j0...", "text": "Seeing examples or trying a small piece of real work — I learn more about how a collaboration works by doing something together.", "orderIndex": 3 },
        { "id": "k1l2...", "text": "Clear ownership and decision boundaries outlined somewhere — I can move quickly once I know who is accountable for what.", "orderIndex": 4 }
      ]
    }
  ]
}
```

Notes:
- `sectionIndex` groups questions into the 10 questionnaire sections for UI progress display
- Dimension weights are **not** included in the response — they are server-side only
- Answer options are returned in `orderIndex` order

**Response 404**: No active questionnaire version exists.

---

## POST /api/responses

Submits a completed response set. Creates a `ResponseSet` with all `Answer`s,
runs the scoring engine, stores `DimensionScore`s, and returns the personal
reflection with credentials.

**Auth**: None required.

**Request**:
```json
{
  "answers": [
    {
      "questionId": "a1b2c3d4-...",
      "primaryAnswerOptionId": "e5f6...",
      "secondaryAnswerOptionId": "g7h8..."
    },
    {
      "questionId": "m3n4o5p6-...",
      "primaryAnswerOptionId": "q7r8...",
      "secondaryAnswerOptionId": null
    }
  ]
}
```

**Validation**:
- Every question from the active questionnaire version must have a `primaryAnswerOptionId`
- `secondaryAnswerOptionId` is optional; omit or set to `null` if not chosen
- If provided, `secondaryAnswerOptionId` must belong to the same question and differ from `primaryAnswerOptionId`
- Each answer option ID must belong to the specified `questionId`
- Duplicate `questionId` entries are rejected

**Response 201**:
```json
{
  "privateResultLink": "/me#TOKEN",
  "accessCode": "K7Q9-MP2D-W4T8",
  "reflection": {
    "groups": [
      {
        "id": "work_context_expectations_and_alignment",
        "title": "Work context, expectations, and alignment",
        "insights": [
          {
            "dimensionId": "clarity_via_written_context",
            "text": "You trust written records more than memory or conversation. Decisions that only exist in someone's head — or were said once in a meeting — are hard for you to rely on.",
            "strength": 4
          },
          {
            "dimensionId": "upfront_clarity_need",
            "text": "You need goals, expectations, and the quality bar to be clear before you commit. Starting without that picture makes it hard to move with confidence.",
            "strength": 3
          }
        ]
      }
    ]
  }
}
```

Notes:
- Only groups where at least one dimension meets the display threshold (normalised score ≥ 0.4) are included
- `strength` is an integer 1–5 derived from the normalised score, used to render a 5-point visual indicator. Never displayed as a number in the UI.
- `privateResultLink` contains the plain token (shown once for bookmarking). Server stores only the hash. After this response, the plain token is gone — the user must save the link.

**Response 400**: Validation failure.
```json
{
  "error": "incomplete_answers",
  "message": "All questions must be answered before submitting."
}
```

**Audit events logged**: `questionnaire_completed`, `personal_reflection_generated`

---

## POST /api/session/start

Validates a private result token (from the URL fragment) and starts a session
by issuing an HttpOnly cookie. Called by the React frontend on page load when
a fragment token is present.

**Auth**: None required.

**Request**:
```json
{
  "token": "raw-token-from-fragment"
}
```

**Response 200**: Empty body. Sets cookie:
```
Set-Cookie: cg_session=<JWT>; HttpOnly; Secure; SameSite=Strict; Max-Age=2592000; Path=/
```

JWT payload (not visible to client):
```json
{
  "sub": "response-set-id",
  "exp": 1234567890
}
```

**Response 401**: Token not found or response deleted.
```json
{
  "error": "invalid_token",
  "message": "This result link is not valid or has been deleted."
}
```

**Audit events logged**: none (session start is not an auditable domain event)

---

## GET /api/me/reflection

Returns the personal reflection for the authenticated session.
Used when a user returns via their saved private result link.

**Auth**: Requires valid `cg_session` cookie (set by `POST /api/session/start`).

**Response 200**:
```json
{
  "reflection": {
    "groups": [
      {
        "id": "work_context_expectations_and_alignment",
        "title": "Work context, expectations, and alignment",
        "insights": [
          {
            "dimensionId": "clarity_via_written_context",
            "text": "You trust written records more than memory or conversation. Decisions that only exist in someone's head — or were said once in a meeting — are hard for you to rely on.",
            "strength": 4
          }
        ]
      }
    ]
  },
  "accessCodeAvailable": true
}
```

Note: The access code itself is not returned here — it was shown once after
completion. The user can request a regenerated code via a separate endpoint
(out of scope for this feature).

**Response 401**: No valid session cookie.
**Response 404**: ResponseSet has been deleted.

---

## Error Response Format

All error responses use a consistent structure:

```json
{
  "error": "snake_case_error_code",
  "message": "Human-readable message safe to show in UI."
}
```

Stack traces, internal IDs, token values, and raw answers are never included
in error responses.
