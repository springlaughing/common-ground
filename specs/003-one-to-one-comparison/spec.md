# Feature Specification: One-to-One Comparison (Core Happy-Path)

**Feature Branch**: `003-one-to-one-comparison`

**Created**: 2026-06-20

**Status**: Draft

**Input**: User description: "One-to-one comparison (core happy-path). Lets two people compare their working styles via an invite, the invitee's explicit consent and completion, automatic deterministic comparison generation, and a neutral comparison report — bilingual, privacy-first, no compatibility score. Reuse-by-access-code, response deletion, group comparison, and raw-answer sharing are explicitly out of scope."

## User Scenarios & Testing *(mandatory)*

This feature lets one person (the **inviter**) compare their working style with exactly one other person (the **invitee**). It builds on the existing personal reflection (feature 001) and bilingual support (feature 002). The four stories below run in flow order; each is independently testable with the prior steps as given-state.

### User Story 1 - Inviter creates an invite (Priority: P1)

A person who has completed the questionnaire and received their private reflection decides to compare with someone. From their result page they create a single-use invite link to share with that one person, and provide a short display name or label for the invitee to recognise them by (e.g. "Alex" or "Team Lead at Acme"). They share only the invite link. They can repeat this to compare with several different people — each invite reuses their own existing response (they never re-take the questionnaire) and forms a separate comparison.

**Why this priority**: Nothing else in the feature can happen without an invite. It is the entry point and establishes the privacy boundary (the invite must not expose the inviter's own results).

**Independent Test**: With a completed reflection, create an invite, confirm a shareable link is produced, confirm the link does not open the inviter's private result page, and confirm a second creation produces a distinct invite without re-taking the questionnaire.

**Acceptance Scenarios**:

1. **Given** an inviter viewing their own reflection, **When** they create an invite and enter a display name for the invitee, **Then** a single-use invite link is generated to share.
2. **Given** a generated invite link, **When** anyone opens it, **Then** it shows the invite/consent entry point and never the inviter's private result page or raw answers.
3. **Given** an invite was created, **When** the inviter views their result page, **Then** they can see that a comparison is pending (awaiting the other person), without seeing who has opened the link.
4. **Given** an inviter who already has one comparison, **When** they create another invite for a different person, **Then** a separate comparison is started reusing their existing response, with no need to re-take the questionnaire.

### User Story 2 - Invitee consents and joins by completing the questionnaire (Priority: P2)

The invited person opens the invite link, sees the inviter's display name and a clear explanation of exactly what will be shared, and must explicitly consent before continuing. After consenting they complete the questionnaire and become a full participant: they receive their own private reflection, their own private result link, and their own access code.

**Why this priority**: This is the privacy-critical step (explicit consent before any sharing) and the step that produces the second response the comparison needs. The invitee is a first-class participant, not just answering on the inviter's behalf.

**Independent Test**: Open a valid invite, verify the consent screen states what is shared and offers accept and decline with equal weight; on decline, nothing is created; on accept, complete the questionnaire and confirm the invitee receives their own private link, access code, and reflection.

**Acceptance Scenarios**:

1. **Given** a valid invite link, **When** the invitee opens it, **Then** they see the inviter's display name and a specific explanation: their answers will be compared with the inviter's, the inviter will see the comparison report but not their raw answers, and audit events are logged.
2. **Given** the consent screen, **When** the invitee declines, **Then** no comparison is created, no response is collected, and nothing is shared.
3. **Given** the invitee has explicitly consented, **When** they complete the questionnaire, **Then** they receive their own private reflection, their own private result link, and their own access code.
4. **Given** an invitee who consented and completed, **When** the invite link is opened again, **Then** it is shown as already used (single-use) and does not start a new session.

### User Story 3 - Comparison generates automatically (Priority: P3)

Once both the inviter's and the invitee's responses exist for the same questionnaire version, the system generates the pair comparison automatically — deterministically, with no human or language-model step.

**Why this priority**: It connects the two responses into the shared result. It is automatic, so it has no UI of its own, but it is the engine that makes the report possible.

**Independent Test**: Given two completed responses on the same questionnaire version, confirm a comparison is produced without any user action, that it is tied to that questionnaire version, and that regenerating from the same responses yields an identical result.

**Acceptance Scenarios**:

1. **Given** the inviter's response exists and the invitee completes theirs on the same questionnaire version, **When** the second response is saved, **Then** the comparison is generated automatically and marked ready.
2. **Given** the same two responses, **When** the comparison is generated more than once, **Then** the result is identical every time (deterministic).
3. **Given** a comparison, **When** it is generated, **Then** it is associated with the questionnaire version both responses share.

### User Story 4 - Both participants view the comparison report (Priority: P1)

Each participant opens their own existing private result link (the `/me` page from features 001/002) and sees their comparison(s) there alongside their personal reflection — that one page becomes their hub, listing every comparison they are part of (pending or ready). Opening a ready comparison shows the report. The report describes each working-style dimension in the second person ("you"/"du") for both people, leads with where the two differ and then where they are similar, writes a shared dimension once, stays neutral and descriptive (no compatibility score, no "fit"/"no-fit"), keeps every insight traceable to questionnaire dimensions, and is available in English or German switchable at view time. It shows summaries, overlaps, differences, and conversation prompts — never raw answers.

**Why this priority**: This is the payoff — the value the whole feature exists to deliver. It is also where the neutrality, privacy, and bilingual guarantees are most visible.

**Independent Test**: With a ready comparison, open each participant's private link and confirm the report renders differ-then-similar, in second person for both, in the viewer's selected language, with no score and no raw answers; switch language and confirm it re-renders.

**Acceptance Scenarios**:

1. **Given** a ready comparison, **When** a participant opens their own private result link (`/me`), **Then** they see the comparison report alongside their personal reflection, on that same page.
2. **Given** a participant who is part of more than one comparison, **When** they open their private result link, **Then** all of their comparisons are listed (each pending or ready), and each ready one can be opened to view its report.
3. **Given** the report, **When** it is displayed, **Then** differences are shown first and similarities second, a shared dimension is written once, and every dimension is phrased in the second person for both people.
4. **Given** the report, **When** the viewer switches language, **Then** all report content re-renders in the selected language with no text from the other language leaking.
5. **Given** the report, **When** it is displayed, **Then** it shows no numeric compatibility score, no "good fit"/"bad fit" verdict, and neither person's raw answers.
6. **Given** a comparison whose data cannot be shown, **When** a participant opens it, **Then** they see a neutral "no longer available" notice rather than an error or partial data.

### Edge Cases

- **Invite expired by time**: opening an invite after its time limit shows a neutral "this invite has expired" message and does not start a session.
- **Invite already used**: a single-use invite that was already joined shows an "already used" message.
- **Expiry mid-completion**: if the invite's time limit passes while the invitee is partway through the questionnaire, their in-progress answers are preserved for a short grace period so they can finish and join.
- **Version mismatch**: the invitee always completes the same questionnaire version the inviter used, so both responses are on the same version; a comparison is never formed across versions.
- **Inviter has no reflection yet**: a person cannot create an invite until they have completed their own response.
- **Comparison not yet ready**: if a participant opens their result before the other has finished, they see a clear "pending — waiting for the other person" state, not an error.
- **Data unavailable**: if a comparison's underlying data is missing, both participants see the neutral "no longer available" notice.
- **Long display name / empty display name**: the inviter's display-name label is bounded and required; the invitee always sees some recognisable label.
- **German length**: longer German report text wraps without overflowing cards (consistent with feature 002's accessibility handling).

## Requirements *(mandatory)*

### Functional Requirements

**Invite (US1)**

- **FR-001**: A participant who has a completed response MUST be able to create one or more invites, each to compare with one other person. Creating an invite reuses the participant's existing response — they MUST NOT have to re-take the questionnaire to start another comparison.
- **FR-002**: Creating an invite MUST require a short display-name label for the invitee, shown to the invitee during consent and used as the invitee's label in the inviter's report.
- **FR-003**: The invite MUST be a shareable link that does not expose the inviter's private result page, reflection, or raw answers.
- **FR-004**: An invite MUST be single-use: once an invitee joins, the invite cannot start another session.
- **FR-005**: An invite MUST expire after a fixed time limit, whichever comes first with single use.
- **FR-006**: The inviter MUST be able to see that a comparison is pending, without being shown whether or when the invite was opened.

**Consent & join (US2)**

- **FR-007**: Opening a valid invite MUST present a consent screen stating specifically what is shared: the invitee's answers are compared with the inviter's, the inviter sees the comparison report but not raw answers, and audit events are recorded.
- **FR-008**: The consent screen MUST present accept and decline with equal visual weight, use specific (not vague) language, avoid urgency/scarcity framing, avoid guilt-tripping on decline, and contain no double negatives.
- **FR-009**: No response MUST be collected and no comparison MUST be created unless the invitee explicitly consents.
- **FR-010**: After consenting and completing the questionnaire, the invitee MUST receive their own private reflection, their own private result link, and their own access code.
- **FR-011**: The invitee MUST complete the same questionnaire version the inviter used.

**Generation (US3)**

- **FR-012**: When both participants' responses exist for the same questionnaire version, the system MUST generate the comparison automatically, with no human or language-model step.
- **FR-013**: The comparison MUST be deterministic: the same pair of responses always produces an identical result.
- **FR-014**: Each comparison MUST be associated with the questionnaire version the two responses share, and MUST NOT be formed from responses on different versions.
- **FR-015**: Every comparison insight MUST be traceable to questionnaire dimensions (explainable).

**Report (US4)**

- **FR-016**: Each participant MUST be able to view their comparison(s) via their own existing private result link — the `/me` page from features 001/002. That page MUST list every comparison the participant is part of (each pending or ready) and show each report there. There MUST be no separate shared comparison link, and the access code MUST NOT be usable to view results.
- **FR-017**: The report MUST describe each dimension in the second person for both people, lead with differences then similarities, and write a shared dimension once.
- **FR-018**: The report MUST be neutral and descriptive: no numeric compatibility score, no "fit"/"no-fit" verdict, no ranking of the people.
- **FR-019**: The report MUST NOT display either participant's raw answers; it shows summaries, overlaps, differences, and conversation prompts only.
- **FR-020**: All report chrome and content MUST be available in English and German, switchable at view time, consistent with feature 002 (a missing translation falls back to English per item).
- **FR-021**: If a comparison cannot be shown, participants MUST see a neutral "no longer available" notice; if it is not yet ready, they MUST see a neutral "pending" state.

**Privacy, consent, audit, accessibility (cross-cutting)**

- **FR-022**: Invite tokens, private result tokens, and access codes MUST be stored only as hashes, never as plain values.
- **FR-023**: The system MUST log lifecycle audit events (invite created, invite opened, comparison joined, comparison generated, access denied, invite expired) and MUST NOT record raw answers, tokens, or access codes in them.
- **FR-024**: The comparison and consent screens MUST be keyboard- and screen-reader-operable and meet the project's WCAG 2.1 AA bar, including the language switcher and longer German text wrapping without overflow.
- **FR-025**: No accounts are introduced; the entire flow remains accountless, and losing both the private result link and the access code has no recovery path (by design).

### Key Entities *(include if feature involves data)*

- **Invite**: a single-use, time-limited invitation created by an inviter. Carries the inviter's display-name label for the invitee, references the inviter's response/comparison, has an expiry and a used/active state, and is identified by a token stored only as a hash.
- **Comparison session**: the pairing of two participants' responses for one questionnaire version, with a status (pending, ready, unavailable). *(Scaffolding exists: `ComparisonSession`.)*
- **Comparison participant**: a participant's membership in a comparison, with a role (initiator or invitee) referencing their response. The inviter's display-name label for the invitee needs a home here or on the invite. *(Scaffolding exists: `ComparisonParticipant`.)*
- **Comparison result**: the deterministic, per-dimension comparison content — for each dimension, both people's strength and the second-person text, grouped and ordered differ-then-similar, in each supported language.
- **Response set**: a participant's completed answers for one questionnaire version (existing entity, reused). One response set may belong to several comparison sessions — this is how a person compares with multiple people without re-taking the questionnaire.
- **Audit event**: a lifecycle record for traceability that never contains raw answers, tokens, or access codes (existing concept, reused).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An inviter with a completed reflection can create and share an invite link in under one minute.
- **SC-002**: After the invitee completes the questionnaire, both participants can open their own private link and see the comparison report with no further action (it is generated automatically).
- **SC-003**: The same pair of responses always produces an identical report — regenerating yields no differences.
- **SC-004**: 100% of report content appears in the viewer's selected language (English or German), switchable at view time, with no text from the other language leaking.
- **SC-005**: The report displays no numeric compatibility score, no "fit"/"no-fit" verdict, and neither participant's raw answers, in 100% of cases.
- **SC-006**: An invite works at most once and stops working after its time limit; an expired or used invite shows a neutral message instead of starting a session.
- **SC-007**: No comparison is created and nothing is shared unless the invitee explicitly consents; declining creates and shares nothing.
- **SC-008**: When a comparison cannot be shown, both participants see a neutral "no longer available" notice rather than an error or partial data, in 100% of cases.
- **SC-009**: Every report insight is traceable to one or more questionnaire dimensions (no unexplained content).
- **SC-010**: A participant can start a comparison with a second (and further) person without re-taking the questionnaire, and their private result link lists all of their comparisons in one place.

## Assumptions

- **Invite lifetime**: defaults to 7 days; the mid-completion grace period is short (finalised during planning). Both are configurable, not user-facing.
- **Inviter prerequisite**: a person must have a completed response before they can create an invite.
- **Two kinds of reuse — only one is in scope**: a participant reusing **their own** response to create several invites (so they can compare with multiple people) is **in scope** — they own their response and never re-take the questionnaire. An **invitee** reusing **their** prior response via access code to skip completing the questionnaire when invited is **out of scope** (feature 004). So in this feature, every invitee completes the questionnaire on joining; the inviter does not.
- **Viewing access**: each participant reaches their comparisons only through their own private result link (the `/me` page), which shows their reflection and lists all their comparisons. There is no shared comparison link; the access code is never a viewing credential (it is a reuse credential, used only in feature 004).
- **Similar vs. differ classification**: a dimension is "similar" when both participants' strengths are aligned (both above or both below the display threshold, within a small gap) and a "difference" when one is above and the other below, or there is a notable strength gap. Exact thresholds reuse the existing reflection scoring/threshold model and are confirmed during planning.
- **Same-version guarantee**: because the invitee completes the inviter's questionnaire version, both responses are always on the same version; cross-version comparison cannot occur.
- **Bilingual reuse**: localization reuses feature 002 (frontend chrome catalogs + database translation tables). Comparison text is second-person and shared by both participants, so there is no third-person variant to author.
- **Display name**: a short, required free-text label provided by the inviter, shown only to the invitee at consent and as the invitee's label in the report; it is not a verified identity.
- **Existing scaffolding**: the `Comparisons` module (`ComparisonSession`, `ComparisonParticipant`) and the frontend `ComparisonPage` (currently rendering demo data) are reused and wired to a real interface; this feature does not reinvent them.

## Out of Scope *(deferred to later features)*

- **Invitee reuse-by-access-code (feature 004)**: an invitee choosing "use my existing response" (enter access code → version check → confirm reuse) to join without completing the questionnaire again. This feature's invitee join step is designed with a clean seam so that "use existing response" can be added alongside "complete the questionnaire" later without rework.
- Deleting a response and the resulting marking of dependent comparisons as unavailable as a **user action** (the unavailable *state* and notice are in scope; the delete action that triggers it is not).
- Group comparison (three or more participants).
- Sharing or revealing raw answers.
- User accounts, access-code regeneration, and report export (PDF/Markdown).
