# API Contract: One-to-One Comparison (Core Happy-Path)

REST over the existing same-origin `/api` surface. All public endpoints are rate-limited; all responses carry the established security headers. Tokens travel in the request body (read from the URL fragment client-side), never in the path/query. `?locale=en|de` follows feature 002 (default `en`, field-level English fallback).

Auth model: the **session cookie** (`cg_session`, from `/session/start`) identifies the caller's `ResponseSetId`. Invite **create** and **`/me/*`** require a session; invite **validate**/**join** are reached from the invite link (no session yet).

## Create an invite — `POST /api/comparisons`

Requires session (the inviter). Creates a `ComparisonSession[Pending]`, the Initiator participant, and an `Active` invite.

Request:
```json
{ "inviterLabel": "Alex" }
```
Response `201`:
```json
{
  "comparisonId": "8f…",
  "inviteToken": "Rk9…",            // plain token — client builds /invite#<token>; never stored plain server-side
  "expiresAt": "2026-06-27T10:00:00Z",
  "status": "pending"
}
```
Errors: `401` no/invalid session · `400` missing/too-long label · `429` rate limited. Audit: `comparison_invite_created`.

## Validate an invite — `POST /api/invite/validate`

No session. Returns the public face of an invite **without consuming it**, so the consent screen can render.

Request:
```json
{ "token": "Rk9…" }
```
Response `200`:
```json
{ "inviterLabel": "Alex", "status": "active", "questionnaireVersion": "1.0.0" }
```
Errors: `404`/`410` unknown / expired / already used → neutral message (no detail leak) · `429`. Audit: `comparison_invite_opened`.

## Join an invite — `POST /api/invite/join`

No prior session. The invitee has consented and completed the questionnaire (the answers are submitted the same way as feature 001's `POST /api/responses`, but bound to this invite and the inviter's version). Consumes the invite single-use, creates the invitee `ResponseSet` + Invitee participant (+ their label), starts the invitee's session, and triggers generation when both responses exist.

Request:
```json
{
  "token": "Rk9…",
  "consent": true,
  "inviteeLabel": "Sam",
  "answers": [ { "questionId": "…", "primaryAnswerOptionId": "…", "secondaryAnswerOptionId": "…" } ]
}
```
Response `201`:
```json
{
  "privateResultLink": "https://…/me#…",   // the invitee's OWN link
  "accessCode": "K7Q9-MP2D-W4T8",
  "comparisonId": "8f…"
}
```
Also sets the invitee's `cg_session` cookie. Errors: `409` invite already used/expired (outside grace) · `400` consent not given / missing label / invalid answers / version mismatch · `429`. Audit: `comparison_joined` (records consent), then `comparison_generated`.

> **Reuse seam (feature 004)**: a later `"useExistingResponse": { "accessCode": "…" }` branch will replace `answers` as an alternate way to supply the invitee's response. The invite/session/generation/report contracts above do not change.

## List my comparisons — `GET /api/me/comparisons`

Requires session. Lists every comparison the caller's response is part of.

Response `200`:
```json
{
  "comparisons": [
    { "comparisonId": "8f…", "otherLabel": "Sam",  "status": "complete", "createdAt": "…" },
    { "comparisonId": "a2…", "otherLabel": "Jordan","status": "pending",  "createdAt": "…" }
  ]
}
```
`status`: `pending` (other not joined yet) · `complete` · `unavailable`.

## Get one comparison report — `GET /api/me/comparisons/{id}?locale=de`

Requires session; the caller's response must be a participant (else `403` + `access_denied` audit). Returns the report from the **caller's** perspective.

Response `200` (matches the frontend `ComparisonDto`):
```json
{
  "otherLabel": "Sam",
  "summary": "You align on 4 of 6 shown dimensions.",
  "groups": [
    {
      "id": "how_you_plan_and_handle_change",
      "title": "How you plan and handle change",
      "insights": [
        { "dimensionId": "planning_boundary_protection", "title": "Protecting the plan from interruption",
          "yourStrength": 4, "theirStrength": 2,
          "yourText": "You need the plan protected…", "theirText": "They can absorb changes…",
          "classification": "difference" }
      ]
    }
  ]
}
```
Ordering: groups/insights present **differences first, then similarities**; a shared dimension appears once. A side below threshold has `null` strength and omitted text. No numeric compatibility score, no fit verdict, no raw answers. States: `pending` → `200` with a pending marker (or `409`); `unavailable` → `200` with the neutral "no longer available" marker. Errors: `401` no session · `403` not a participant · `404` unknown id.

## Notes

- The inviter sees a comparison as `pending` on their `/me` hub immediately after creating the invite; they are never told whether/when the link was opened (FR-006).
- Generation is deterministic and computed on read; `comparison_generated` is audited once when the session first reaches `complete`.
- All endpoints validate input at the boundary and return neutral, detail-free error bodies.
