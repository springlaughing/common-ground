# API Contracts: Bilingual (English/German) Support

**Feature**: 002-bilingual-support | **Date**: 2026-06-12 | **Base URL**: `/api`

Extends the [001 API contracts](../001-questionnaire-completion/contracts/api.md). Only
**changes** are shown. All three content endpoints gain an optional `locale` query
parameter.

**`locale` parameter (shared rules)**:
- Accepted values: `en`, `de`. Default when omitted: `en`.
- Unknown/unsupported values are treated as `en` (no error response).
- Affects **display text only** — never which insights appear, their order, or their strength.
- Per-field English fallback when a translation is missing.

---

## GET /api/questionnaire/current?locale=en|de

Returns the active questionnaire with question and answer-option text in the requested
locale. Structure, IDs, `sectionIndex`, and `orderIndex` are identical across locales;
**only `text` values differ**. Dimension weights remain server-side only.

**Example** `GET /api/questionnaire/current?locale=de` →
```json
{
  "id": "3fa85f64-...",
  "versionNumber": "1.0",
  "questions": [
    {
      "id": "a1b2c3d4-...",
      "text": "Wenn du mit jemandem neu zusammenarbeitest, was hilft dir, gut ins gemeinsame Arbeiten zu kommen?",
      "sectionIndex": 1,
      "orderIndex": 1,
      "answerOptions": [
        { "id": "e5f6...", "text": "Schriftliche Ziele, Kontext und Erwartungen — …", "orderIndex": 1 }
      ]
    }
  ]
}
```
The `id` values are identical to the English response — this is what lets a client switch
language mid-flow and keep already-selected answers.

---

## POST /api/responses?locale=en|de

Unchanged request body and validation. The returned `reflection` is rendered in `locale`.
Each insight now includes a localized **`title`**.

**Response 201** (German example, abridged):
```json
{
  "privateResultLink": "/me#TOKEN",
  "accessCode": "K7Q9-MP2D-W4T8",
  "reflection": {
    "groups": [
      {
        "id": "work_context_expectations_and_alignment",
        "title": "Arbeitskontext, Erwartungen und Abstimmung",
        "insights": [
          {
            "dimensionId": "clarity_via_written_context",
            "title": "Schriftliches vor Gedächtnis",
            "text": "Du vertraust schriftlichen Aufzeichnungen mehr als dem Gedächtnis …",
            "strength": 4
          }
        ]
      }
    ]
  }
}
```
- **`title`** (NEW): the short, localized dimension title. Previously absent from the API;
  now populates the existing title slot on the reflection card. Falls back to English if a
  locale title is missing.
- `strength` is locale-invariant. For identical answers, the set of insights, their order,
  and strengths are identical to the English response (SC-003).

**Audit events logged**: `questionnaire_completed`, `personal_reflection_generated`
(unchanged — no locale recorded).

---

## GET /api/me/reflection?locale=en|de

Returns the personal reflection for the authenticated session in the requested locale.
Because **no locale is stored on the response**, the same saved reflection can be requested
in either language — the client re-calls this endpoint with a different `locale` when the
user switches language while viewing (User Story 4).

**Response 200**: same shape as `POST /api/responses` `reflection` (groups → insights with
localized `title` + `text`, locale-invariant `strength`), plus `accessCodeAvailable`.

**Response 401**: no valid session cookie. **Response 404**: response deleted. (Unchanged.)

---

## Unchanged endpoints

`POST /api/session/start` is unaffected (no localized content; the fragment-token →
cookie flow is identity, not display). Error response format is unchanged; error
`message` values may themselves be localized client-side from error `code`s, but the
contract shape is the same.
