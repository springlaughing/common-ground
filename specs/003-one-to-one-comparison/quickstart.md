# Quickstart: One-to-One Comparison (Core Happy-Path)

How to run, exercise, and verify the comparison flow locally.

## Run locally

Backend + DB (from `backend/`), then frontend (from `frontend/`):

```powershell
# Backend (applies the AddComparisons migration against local Postgres)
dotnet run --project src/CommonGround.Api          # http://localhost:5148

# Frontend
npm run dev                                         # http://localhost:5173
```

## Walk the flow (two browser profiles = two people)

1. **Person A (inviter)**: complete the questionnaire → reflection → on `/me`, choose **Invite someone**, enter a label for yourself (e.g. "Alex"). Copy the invite link (`/invite#…`).
2. **Person B (invitee)**: open the invite link in a second profile. You see "Alex invited you…", the consent screen (accept/decline equal weight). **Decline** → nothing happens. **Accept**, enter your own label (e.g. "Sam"), complete the questionnaire → you receive your **own** `/me#…` link + access code.
3. **Both**: open your own `/me` link → the comparison now shows **Complete**; open it → the report (differences first, then similarities; second person for both; your selected language). Switch language → it re-renders.
4. **Multi-comparison**: as Person A, create a second invite (no re-taking the questionnaire); Person C joins → your `/me` lists **two** comparisons.

## Try the API directly

```
POST /api/comparisons            { "inviterLabel": "Alex" }        # (session cookie) -> inviteToken
POST /api/invite/validate        { "token": "…" }                  # inviter label, no consume
POST /api/invite/join            { "token","consent":true,"inviteeLabel","answers":[…] }
GET  /api/me/comparisons                                            # (session) list
GET  /api/me/comparisons/{id}?locale=de                             # (session) report, German
```

## What to verify (maps to Success Criteria)

- **SC-001**: From a finished reflection, create + copy an invite in well under a minute.
- **SC-002**: After B completes, both A and B see the report on their own `/me` with no extra action.
- **SC-003**: Re-fetch the report repeatedly → byte-identical (deterministic; computed on read).
- **SC-004**: Switch the report EN⇄DE → all content re-renders in the chosen language, no leakage.
- **SC-005**: Report shows no numeric score, no "fit"/"no-fit", and neither person's raw answers.
- **SC-006**: Open the invite a second time after joining → "already used"; let an invite pass 7 days → "expired".
- **SC-007**: Decline at consent → no comparison, no response, nothing shared.
- **SC-008**: Force a dependency missing → both see the neutral "no longer available" notice.
- **SC-009**: Every report line maps to a dimension id.
- **SC-010**: Create a second comparison without re-taking the questionnaire; `/me` lists all of them.

## Tests

```powershell
# Backend
dotnet test                      # engine (branches/symmetry/determinism), integration flow, arch boundaries
# Frontend
npm run test                     # invite-create, comparison list, consent variant, report rendering
npm run test:e2e                 # create invite -> invitee consent+complete -> both view -> second comparison listed
```

Key cases: engine classification per dimension (similar/difference/omitted) + symmetry (A↔B = B↔A per viewpoint) + determinism; invite single-use + expiry + grace; same-version guard; consent required before any write; report has no score/raw answers; `/me` hub lists multiple comparisons; bilingual report with English fallback; audit events written without sensitive content.

## Add the invitee reuse path later (feature 004)

The invitee join step is shaped so "use existing response" (enter access code → version check → confirm reuse with [label]) slots in next to "complete the questionnaire" without changing the invite, session, generation, or report contracts. See [research.md](research.md) §10.
