# Research: One-to-One Comparison (Core Happy-Path)

Phase 0 decisions. Each resolves a design unknown from the spec/plan with a rationale and the alternatives weighed. No open `NEEDS CLARIFICATION` remain.

## 1. Comparison classification — similar vs. differ

**Decision**: Reuse the existing reflection per-dimension **strength** (computed by the scoring engine; a dimension is "shown" when its strength is at or above the existing display threshold, otherwise treated as `null`). For a pair, per dimension:
- both at/above threshold and strengths within a small gap (|Δ| ≤ 1) → **similar (aligned)**;
- both at/above threshold with a notable gap (|Δ| ≥ 2), or one at/above and the other below (`null`) → **difference**;
- both below threshold → **omitted** (neither has a clear signal — consistent with the reflection only surfacing clear patterns).
The report lists **differences first, then similarities**; a dimension both share is written once.

**Rationale**: Reuses the deterministic, versioned scoring already trusted by the personal reflection; needs no new scoring rules; produces explainable, per-dimension output (Principle III). Mirrors the demo `ComparisonDto` (`yourStrength`/`theirStrength`, `null` = below threshold).

**Alternatives considered**: A continuous "alignment %" per dimension or overall — rejected (drifts toward a score; Principle II prohibits overall scores and the per-dimension bucket is clearer). A separate comparison-only scoring pass — rejected (duplicates logic, risks divergence from the reflection).

## 2. Comparison result — computed on read, not stored

**Decision**: Persist only the pairing (`ComparisonSession` + two `ComparisonParticipant`s). Compute the report on each read from the two responses' dimension scores + localized insight text. Nothing about the report is cached.

**Rationale**: The engine is deterministic over versioned inputs, so a stored copy could only ever match the computed one — caching adds staleness risk and invalidation complexity for no benefit. Guarantees SC-003 by construction.

**Alternatives considered**: A `ComparisonResult` table (cache) — rejected per above. Materializing on generation for "speed" — unnecessary; inputs are tiny.

## 3. Invite token — fragment-delivered, server-validated, single-use, time-limited

**Decision**: Generate the invite token with the existing `TokenService.GenerateToken()` (32 bytes, base64url) and store only `HashToken(...)`. Deliver it in the **URL fragment** (`/invite#TOKEN`), like `/me#TOKEN`. The `/invite` SPA reads the fragment and POSTs the token to a **validate** endpoint that returns only the inviter's display label + invite state (does **not** consume it); joining POSTs again to **consume** it (single-use). Expiry = fixed window (default **7 days**) from creation; a **grace period** (default **30 minutes**) lets an invitee who has already started the questionnaire finish after the window passes. An invite becomes `Used` on successful join.

**Rationale**: Keeps a live credential out of server/proxy logs (Principle VI's fragment rationale), reusing the proven `/me` pattern and the same hashing. Validate-without-consume lets the consent screen render before the irreversible single-use step.

**Alternatives considered**: `/invite/{token}` path (product-concept sketch) — rejected (logs the token). JWT invite — rejected (heavier; no need; hash lookup suffices). Consuming on first open — rejected (would burn the invite before consent).

## 4. Display labels — one self-label per participant, consent-disclosed

**Decision**: Each participant provides a short, required, non-verified **display label** for the *other* to see: the **inviter** sets theirs when creating the invite (shown to the invitee at consent: "[Label] invited you…"); the **invitee** sets theirs when joining. Each report names "the other person" by that label; own side is always second person ("you"). The invitee's consent copy states that their chosen label is shown to the inviter.

**Rationale**: The report needs a label for the other person from both viewpoints (matches `ComparisonDto.theirName`); a single inviter-only label leaves the inviter's view of the invitee unnamed. This is the **first personal-ish field** the product collects (feature 002 collected none) — it is opaque, user-chosen, optional in spirit, and disclosed at consent (Principle V). Recorded for an ADR.

**Alternatives considered**: Inviter labels both sides — rejected (awkward; the invitee should name themselves). No labels ("the other person") — rejected (impersonal; contradicts the existing report shape). Collecting a real name — rejected (privacy; not needed).

## 5. Identity for the `/me` hub — session → ResponseSet → comparisons

**Decision**: The existing `cg_session` cookie identifies the viewer's `ResponseSetId` (issued by `/session/start` from their `/me#TOKEN`). `GET /api/me/comparisons` lists every `ComparisonSession` containing that response (each with the other participant's label + status); `GET /api/me/comparisons/{id}?locale=` returns the report from **that** participant's perspective ("you" = the session's response). Access is denied (and audited) if the session's response is not a participant.

**Rationale**: Reuses 001/002's accountless session model unchanged; the `/me` page already starts a session, so the hub is a natural extension. Per-viewer perspective gives the second-person-for-both report without storing perspective.

**Alternatives considered**: A shared comparison link both open — rejected (Principle VI: each person accesses only via their own private link; no shared credential). Embedding viewer identity in the URL — rejected (the cookie already carries it safely).

## 6. Comparison report text — reuse Reporting's localized insight snippets

**Decision**: The per-dimension text in the report is the **existing localized insight snippet** for that dimension (already authored EN/DE, second person "you"/"du", seeded in feature 002). The comparison assembler pulls title + text per dimension from `Reporting` (via interface) and pairs it with the engine's classification + each side's strength. **No new comparison content is authored.** An optional, neutral one-line summary may state "you align on N of M dimensions" (a per-dimension count, explicitly allowed by Principle II — not a score).

**Rationale**: The reflection text is already second-person and dimension-keyed, so it serves both "you" and "them" verbatim (the user confirmed comparison voicing is 2nd-person for both, no third-person variant). Zero new content, fully bilingual via 002's fallback.

**Alternatives considered**: Authoring dedicated comparison sentences per dimension/relationship — rejected for MVP (large content effort; the existing snippets already read correctly in this layout, per the demo). Third-person ("they") variant — rejected (user decision: 2nd person both).

## 7. Audit events

**Decision**: Append-only `AuditEvent`s (existing entity already carries `ComparisonSessionId`): `comparison_invite_created`, `comparison_invite_opened`, `comparison_joined` (records consent given), `comparison_generated`, `access_denied`, `invite_expired`. Metadata holds only non-sensitive context (e.g., session id) — never raw answers, tokens, or labels-as-PII beyond what's needed; never the token/code.

**Rationale**: Satisfies Principle I/V/VII audit requirements and the product concept's event list; reuses the existing `IAuditLogger`/`EfAuditLogger`.

## 8. Rate limiting & security baseline

**Decision**: Apply the existing `PostPolicy` rate limiter to invite **create**, **validate**, and **join** endpoints (public-facing). Reuse the established security headers/HTTPS/session-cookie posture. (Access-code entry rate limiting lands with reuse in feature 004.)

**Rationale**: Constitution security baseline — rate-limit all public endpoints; invite validation is brute-forceable otherwise.

## 9. Same-version enforcement

**Decision**: The invitee always completes the **inviter's** `QuestionnaireVersionId` (carried on the invite/session). The comparison is formed only from two responses on that version; generation asserts the version match. Cross-version comparison cannot arise in the happy path; the assertion guards against future paths (e.g., reuse in 004).

**Rationale**: Determinism + explainability require a single scoring basis; the constitution makes versions immutable once responses exist.

## 10. Reuse seam (feature 004)

**Decision**: Model the invitee join as an explicit step that today has one path ("complete the questionnaire"). Keep the join endpoint shaped so a second path ("use existing response" via access code → version check → confirm) can be added without changing the invite, session, generation, or report code.

**Rationale**: The user deferred reuse-by-access-code to 004 but wants no rework; isolating "how the invitee's response is obtained" behind the join step achieves that.
