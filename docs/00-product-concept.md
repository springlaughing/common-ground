# CommonGround App — Product Concept

## 1. One-sentence summary

A privacy-conscious mutual reflection app that helps people compare collaboration styles, working preferences, and engineering culture expectations through neutral questionnaire-based insights.

---

## 2. Problem statement

People often start working together without clearly understanding each other's expectations around communication, feedback, quality, decision-making, conflict, deadlines, and collaboration.

Traditional "culture fit" discussions can be vague, biased, or one-sided. The goal of this app is to make these expectations easier to discuss in a structured, respectful, and non-judgmental way.

---

## 3. Product goal

The app helps two or more people compare self-reported working-style preferences and receive:

- personal reflection
- shared patterns
- meaningful differences
- neutral conversation prompts

---

## 4. Non-goals

The app is not:

- a psychological assessment
- an emotional intelligence test
- a hiring decision tool
- a personality diagnosis
- a compatibility score generator
- a social network
- a dating app
- a replacement for real conversation

---

## 5. Target users

- job candidate and potential team lead
- manager and team member
- teammates
- mentor and mentee
- cofounders
- collaborators
- open-source maintainers and contributors
- small teams (via pairwise comparisons in MVP; group report is a later feature)

---

## 6. Core use cases

### Use case 1: Personal reflection

A user fills in the questionnaire and receives a private reflection page showing their own working-style patterns.

The reflection is accessible only through the user's private result link.

---

### Use case 2: One-to-one comparison

A user creates an invite link and shares it with one other person.

The invited person completes the questionnaire or reuses an existing response with explicit consent.

Once both responses are available, the app automatically generates a pair comparison report. Both participants access the report independently via their own private result links.

The report shows:

- shared working-style patterns
- meaningful differences
- neutral conversation prompts
- explanations based on questionnaire answers or dimensions

---

### Use case 3: Response reuse across multiple comparisons

A user should not be forced to fill in the same questionnaire again if they already completed it.

A completed response can be reused across multiple separate comparisons, provided it was completed for the same questionnaire version. If a newer questionnaire version exists, the user must fill in a new response — old responses cannot be reused across versions.

For example, one person may compare their response with a team lead, a teammate, a mentor, and a recruiter without filling in the questionnaire again each time. Each comparison remains separate.

Reuse is always user-initiated. The app does not automatically identify returning users. An invitee who wants to reuse an existing response must choose "Use existing response" and enter their access code. The backend hashes the entered code, looks for a matching response, checks that the questionnaire version matches the invite, and verifies the response has not been deleted. Only then does it ask the user to confirm reuse.

---

### Use case 4: Invited participant gets their own private result

An invited participant is not just filling in the questionnaire for someone else.

After completing the questionnaire, the invited participant also receives:

- their own private reflection page
- their own private result link
- their own access code
- access to the comparison report when it is ready

---

### Use case 5: Group comparison *(post-MVP)*

A small group can compare shared and different working preferences in one group report.

For example, a manager and several teammates may join the same comparison session.

The group report may show:

- where most participants have similar preferences
- where the group is split
- where there are different but complementary working styles
- conversation prompts for team alignment

The MVP supports personal reflection, reusable responses, and one-to-one comparison only. Small teams can use the app in MVP by running pairwise comparisons between members. The data model should support group comparisons from the beginning so this feature can be added without a migration.

---

### Use case 6: Delete personal response and comparison access

A participant can delete their own response.

When a response is deleted:

- the participant's private reflection page is no longer available
- the response can no longer be reused in new comparisons
- access through the private result link is disabled
- comparisons that depend on this response are marked as unavailable — they remain as records but are inaccessible to all participants, who see a notice that the comparison is no longer available

The app should make the deletion behavior clear before the user confirms deletion.

For MVP, deleting a response removes the user's answers and disables future access to related personal results. Audit events may keep a minimal record that deletion happened, but must not store raw answers.

---

## 7. Main user flows

### First participant flow

1. User opens app
2. Reads privacy and consent explanation
3. Fills in questionnaire
4. Receives personal reflection
5. Receives private result link
6. Receives access code
7. Creates invite link (provides a display name or label for the invitee to see)
8. Shares only the invite link with another person

---

### Invited participant flow

1. Invited person opens `/invite/...`
2. Reads explanation of what will be shared
3. Explicitly consents to:
   - their answers being compared with the inviter's
   - the inviter seeing the comparison report (not raw answers)
   - audit events being logged for traceability
   - their response being used for this specific comparison only (if reusing)
4. Chooses one option:
   - Fill in questionnaire
   - Use existing response (enter access code)
5. Receives their own private result link
6. Receives their own access code
7. Sees personal reflection
8. Sees comparison report when it is ready

---

### Reuse sub-flow (step 4b above)

1. Invitee selects "Use existing response"
2. App prompts: "Enter your access code"
3. Invitee enters code (e.g. `K7Q9-MP2D-W4T8`)
4. App finds matching response and checks questionnaire version
5. App asks: "Use this response for this comparison?"
6. Invitee confirms
7. Response is attached to the comparison — no second questionnaire needed

---

## 8. Key product rules and constraints

### Rule 1: Invite link is only for inviting

The link shared with another person is an invite link:

```
/invite/{inviteToken}
```

It does not expose the inviter's private result page.

When creating the invite link, the inviter provides a display name or label (e.g. `"Alex"` or `"Team Lead at Acme"`). This name is shown to the invitee during the consent step.

**Expiry:** Invite links expire under two conditions, whichever comes first:

- a fixed time limit (e.g. 7 days)
- the invitee successfully joins the comparison (single-use)

If the invitee opens the link and begins filling in the questionnaire but the invite expires mid-flow, their in-progress answers are preserved for a short grace period so they can complete the questionnaire. Once the session completes, the invite is consumed.

**Visibility:** The `comparison_invite_opened` audit event is logged internally for traceability. It is not surfaced to the inviter in the UI. The inviter only sees when the comparison report is ready.

---

### Rule 2: Each participant gets their own private result link

After completing the questionnaire, every participant receives their own private result link:

```
/me/{privateResultToken}
```

The private result link is the primary way to access personal results and existing comparisons. It is distinct from the access code — the access code is used only to reuse a response in a new comparison (see Rule 5), not to open the result page.

If a participant loses both their private result link and their access code, there is no recovery path. Because the MVP has no accounts, the app cannot verify identity. The participant must fill in a new response. This is by design and consistent with the privacy-first approach.

The result page shows:

- personal reflection
- existing comparisons
- pending comparisons
- ability to create new comparisons
- access code reminder or regeneration option (regenerating invalidates the old code immediately)
- delete response option

---

### Rule 3: A response can be reused

Core model:

- `ResponseSet` — one person's completed answers to one questionnaire version
- `ComparisonSession` — a comparison that references two or more ResponseSets

The same `ResponseSet` can be used in multiple `ComparisonSession`s, as long as the questionnaire version matches.

---

### Rule 4: Reuse requires explicit consent

Even if the app finds an existing response by access code, it must not automatically attach it to a new comparison.

The user must confirm:

> Use this existing response for comparison with **[Name]**?

`[Name]` is the display name the inviter provided when creating the invite link.

---

### Rule 5: Access code supports accountless reuse

Because the MVP has no accounts, users reuse previous responses by entering an access code:

```
K7Q9-MP2D-W4T8
```

The access code is a portable credential used only to attach an existing response to a new comparison. It does not open the personal result page — that requires the private result link.

The access code is shown after questionnaire completion and explained clearly, including its distinction from the private result link.

---

### Rule 6: Access code must be private

The UI should warn:

> Keep this access code private. Anyone with this code can reuse your response in a new comparison.

The app stores only a hash of the access code, never the plain value.

---

### Rule 7: Raw answers are not shown by default

Comparison reports show summaries, overlaps, differences, and prompts.

Raw answers are not visible unless a future feature explicitly allows sharing them with consent.

---

### Rule 8: No compatibility score

The app avoids language such as:

- `87% compatible`
- `good fit` / `bad fit`
- `high empathy` / `low empathy`
- `hire` / `reject`

Instead it uses neutral, descriptive language:

- *You both prefer explicit expectations.*
- *You differ in how much flexibility you prefer under deadline pressure.*
- *This may be useful to discuss before working together.*

---

### Rule 9: Results are explainable

Each insight should be traceable to questionnaire dimensions or specific questions.

Example:

> *Based on your answers about feedback rhythm and decision-making, you both prefer written context before important discussions.*

---

### Rule 10: Context matters

A response may depend on context. Someone may answer differently for a job interview, a current team, a cofounder relationship, or a mentorship.

Reuse options (full set, future):

- Use existing response
- Review/edit a copy
- Fill in a new response

**MVP reuse options:**

- Use existing response
- Fill in again

"Review/edit a copy" is documented as a future improvement.

---

## 9. Privacy and ethics principles

This application is a reflective comparison tool. It does not diagnose personality, empathy, morality, emotional intelligence, mental health, or relationship compatibility. Results are based only on self-reported answers and are presented as neutral conversation prompts.

The app is built around these principles:

- no psychological diagnosis
- no ranking or scoring people
- no hidden sharing
- no raw answer sharing by default
- explicit consent before reuse, scoped to the specific comparison
- private result links
- access codes treated as sensitive credentials
- ability to delete response at any time
- invite links are time-limited and single-use
- minimal data collection

---

## 10. MVP scope

**In scope:**

- questionnaire definition
- questionnaire completion
- personal reflection result
- private result link
- access code generation
- invite link generation (with inviter display name)
- one-to-one comparison
- automatic comparison generation when both responses are available
- reuse existing response by access code
- explicit consent before reuse
- comparison result page
- delete response
- invite expiry (time-based and single-use)
- audit event logging

**Out of scope:**

- user accounts
- social network profiles
- public search
- chat
- AI-generated psychological analysis
- employer ranking
- real hiring recommendations
- complex admin dashboard
- microservices

---

## 11. Future ideas

- optional accounts and email magic links
- group comparisons (single report for three or more participants)
- PDF or Markdown report export
- questionnaire version management UI
- organization and team spaces
- optional raw answer sharing with consent
- analytics for small teams
- reporting module
- review/edit a copy of an existing response before reuse

---

## 12. Technical implications

The product should be designed as a **modular monolith**.

**Modules:**

- `Questionnaires`
- `Responses`
- `Comparisons`
- `Reporting`
- `Privacy`
- `Audit`
- `Notifications`

**Domain objects:**

- `QuestionnaireTemplate`
- `QuestionnaireVersion`
- `Question`
- `AnswerOption`
- `ScoringRule`
- `InsightTemplate`
- `ResponseSet`
- `Answer`
- `ComparisonSession`
- `ComparisonParticipant`
- `ComparisonResult`
- `InviteToken`
- `AccessCode`
- `AuditEvent`

**Key technical decisions:**

- deterministic comparison engine
- no LLM-generated interpretation in MVP
- access code stored as hash
- private result tokens stored as hash
- comparison results tied to questionnaire version
- audit events record important lifecycle actions
- architecture tests enforce module boundaries

---

## 13. Audit events

The app logs important lifecycle events for traceability, but never raw answers.

**Events logged:**

- `questionnaire_started`
- `questionnaire_completed`
- `personal_reflection_generated`
- `comparison_invite_created`
- `comparison_invite_opened`
- `comparison_joined`
- `existing_response_reuse_requested`
- `existing_response_reuse_approved`
- `comparison_generated`
- `response_deleted`
- `comparison_deleted`
- `access_denied`
- `invite_expired`

**Audit logs must be able to answer:**

- Was consent given?
- Was a response reused?
- Was a comparison generated?
- Was an invite expired?
- Was a response deleted?
- Was access denied?

**Audit logs must never contain:**

- raw answers
- private tokens
- access codes
- full result text
- sensitive personal content
