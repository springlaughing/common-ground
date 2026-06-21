# Data Model: One-to-One Comparison (Core Happy-Path)

Owning module: **Comparisons** (entities + engine). Insight text is read from **Reporting** via interface; responses/scores from **Responses**/**Reporting**; tokens/hashing from **Privacy**; lifecycle records in **Audit**. Nothing about the computed report is persisted (see [research.md](research.md) §2).

## New / changed entities

### Invite *(new — Comparisons)*

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `ComparisonSessionId` | Guid | FK → ComparisonSession (created together with the invite) |
| `InviterResponseSetId` | Guid | the creator's response (also the Initiator participant) |
| `InviterLabel` | string(≤ 60) | inviter's self-label, shown to the invitee at consent; required |
| `TokenHash` | string | HMAC-SHA256 of the invite token (plain token only in the link fragment); unique |
| `Status` | enum | `Active` · `Used` · `Expired` |
| `ExpiresAt` | DateTimeOffset | created + fixed window (default 7d) |
| `CreatedAt` | DateTimeOffset | |

**Rules**: single-use — `Active → Used` atomically on successful join; reads treat `now > ExpiresAt` as expired (and may lazily flip to `Expired` + emit `invite_expired`). A started-but-unfinished invitee within the grace window (default 60 min past `ExpiresAt`) may still complete. The token is never stored in plain text.

### ComparisonSession *(existing entity — add EF config + migration)*

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `QuestionnaireVersionId` | Guid | the shared version; both participants must match |
| `Status` | enum | `Pending` (awaiting invitee) · `Complete` (both joined → report available) · `Unavailable` (a dependency missing) |
| `CreatedAt` | DateTimeOffset | |
| `Participants` | collection | 1..N `ComparisonParticipant` (2 in this feature; N-ready per Principle IV) |

### ComparisonParticipant *(existing entity — add `DisplayLabel` + EF config)*

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `ComparisonSessionId` | Guid | FK |
| `ResponseSetId` | Guid | the participant's response |
| `Role` | enum | `Initiator` · `Invitee` (existing) |
| `DisplayLabel` | string(≤ 60) | **new** — this participant's self-label, shown to the *other* participant; the Initiator's is copied from `Invite.InviterLabel`, the Invitee's is captured at join |
| `JoinedAt` | DateTimeOffset | |

### AuditEvent *(existing — reused, no schema change)*

Already carries `ComparisonSessionId` and `Metadata`. New `EventType` values: `comparison_invite_created`, `comparison_invite_opened`, `comparison_joined` (consent given), `comparison_generated`, `access_denied`, `invite_expired`. Metadata holds non-sensitive context only — never raw answers, tokens, or access codes.

### ResponseSet *(existing — reused unchanged)*

One `ResponseSet` may be referenced by several `ComparisonParticipant`s across sessions — this is how one person compares with multiple people without re-taking the questionnaire. Carries `QuestionnaireVersionId`, `PrivateResultTokenHash`, `AccessCodeHash`, `IsDeleted`.

## Computed (not persisted) — the report

Assembled on read by the comparison assembler:

- **ComparisonReport**(viewerParticipantId, otherLabel, groups[]) where each group has differences-first then similarities, each insight = `{ dimensionId, title, yourStrength, theirStrength, yourText?, theirText?, classification }`.
- `yourStrength`/`theirStrength` are the two responses' per-dimension strengths (`null` below threshold); `yourText`/`theirText` are the existing localized insight snippets for that dimension (omitted for a side scoring below threshold). Shape matches the frontend `ComparisonDto`/`ComparisonInsightDto` already present.
- Optional neutral summary: "you align on N of M shown dimensions" (count, not a score).

## Lifecycle / state

```
Inviter (has ResponseSet)
  └─ create invite ──▶ ComparisonSession[Pending] + Participant[Initiator] + Invite[Active]   (audit: invite_created)
Invitee opens /invite#TOKEN
  └─ validate (no consume) ──▶ shows InviterLabel + consent                                    (audit: invite_opened)
     ├─ decline ──▶ nothing created
     └─ consent + complete questionnaire ──▶ ResponseSet(invitee) + Participant[Invitee]+label
                                              Invite[Active→Used]                                (audit: invite_joined)
Both responses present (same version)
  └─ generate (automatic, on read or on join) ──▶ ComparisonSession[Complete]                   (audit: comparison_generated)
Either participant opens /me
  └─ list comparisons; open one ──▶ report computed for that viewer
Dependency missing (e.g., future deletion) ──▶ ComparisonSession[Unavailable] ──▶ neutral notice
Invite past window (no active grace) ──▶ Invite[Expired]                                         (audit: invite_expired)
```

## Constraints & indexes

- `Invite.TokenHash` unique, indexed (lookup on validate/join).
- `ComparisonParticipant (ComparisonSessionId, ResponseSetId)` unique; index `ResponseSetId` (the `/me` hub query).
- A `ComparisonSession` reaches `Complete` only with two participants whose responses share `QuestionnaireVersionId`.
- All token/code values are hashes; labels are bounded (≤ 60 chars), required, non-verified.
