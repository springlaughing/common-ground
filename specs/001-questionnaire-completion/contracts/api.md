# API Contracts: Questionnaire Completion

**Feature**: 001-questionnaire-completion
**Date**: 2026-05-14
**Base URL**: `/api`

All requests and responses use JSON. All endpoints require HTTPS.
Timestamps are ISO 8601 UTC strings.

---

## GET /api/questionnaire/current

Returns the active questionnaire version with all questions and answer options.
Used by the frontend to render the questionnaire.

**Auth**: None required.

**Response 200**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "versionNumber": "1.0",
  "questions": [
    {
      "id": "a1b2c3d4-...",
      "text": "How do you prefer to receive feedback on your work?",
      "dimension": "FeedbackRhythm",
      "orderIndex": 1,
      "answerOptions": [
        { "id": "e5f6...", "text": "In writing, so I can read it at my own pace", "orderIndex": 1 },
        { "id": "g7h8...", "text": "In a short verbal conversation", "orderIndex": 2 },
        { "id": "i9j0...", "text": "As part of a regular scheduled review", "orderIndex": 3 },
        { "id": "k1l2...", "text": "Informally, whenever it comes up naturally", "orderIndex": 4 }
      ]
    }
  ]
}
```

Note: `ScoringValue` is **not** included in the response — it is server-side only.

**Response 404**: No active questionnaire version exists.

---

## POST /api/responses

Submits a completed response. Creates a `ResponseSet` with all `Answer`s,
runs the scoring engine, and returns the personal reflection with credentials.

**Auth**: None required.

**Request**:
```json
{
  "answers": [
    { "questionId": "a1b2c3d4-...", "answerOptionId": "e5f6..." },
    { "questionId": "m3n4o5p6-...", "answerOptionId": "q7r8..." }
  ]
}
```

**Validation**:
- All questions from the active questionnaire version must be answered
- Each `answerOptionId` must belong to the specified `questionId`
- Duplicate `questionId` entries are rejected

**Response 201**:
```json
{
  "privateResultLink": "/me#TOKEN",
  "accessCode": "K7Q9-MP2D-W4T8",
  "reflection": {
    "insights": [
      {
        "dimension": "FeedbackRhythm",
        "text": "You prefer to receive feedback in writing, giving you time to reflect before responding."
      },
      {
        "dimension": "DecisionMaking",
        "text": "You tend to gather input from others before committing to important decisions."
      }
    ]
  }
}
```

Note: `privateResultLink` contains the plain token (shown once to the user for
bookmarking). The server stores only the hash. After this response, the plain
token is gone — the user must save the link.

**Response 400**: Validation failure — missing answers, invalid option IDs, duplicates.
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
    "insights": [
      {
        "dimension": "FeedbackRhythm",
        "text": "You prefer to receive feedback in writing, giving you time to reflect before responding."
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
