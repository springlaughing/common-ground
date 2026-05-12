# CommonGround App — Product Concept

## 1. One-sentence summary
A privacy-conscious mutual reflection app that helps people compare collaboration styles, working preferences, and engineering culture expectations through neutral questionnaire-based insights.

## 2. Problem statement
People often start working together without clearly understanding each other’s expectations around communication, feedback, quality, decision-making, conflict, deadlines, and collaboration.

Traditional “culture fit” discussions can be vague, biased, or one-sided. The goal of this app is to make these expectations easier to discuss in a structured, respectful, and non-judgmental way.

## 3. Product goal
The app helps two or more people compare self-reported working-style preferences and receive:

- personal reflection
- shared patterns
- meaningful differences
- neutral conversation prompts

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

## 5. Target users
Possible users:

- job candidate and potential team lead
- manager and team member
- teammates
- mentor and mentee
- cofounders
- collaborators
- open-source maintainers and contributors
- small teams

## 6. Core use cases
### Use case 1: Personal reflection

A user fills in the questionnaire and receives a private reflection page showing their own working-style patterns.

The reflection is visible only through the user’s private result link or access code.

### Use case 2: One-to-one comparison

A user creates an invite link and shares it with one other person.

The invited person completes the questionnaire or reuses an existing response with explicit consent.

Once both responses are available, the app generates a pair comparison report showing:

- shared working-style patterns
- meaningful differences
- neutral conversation prompts
- explanations based on questionnaire answers or dimensions

### Use case 3: Response reuse across multiple comparisons

A user should not be forced to fill in the same questionnaire again if they already completed it.

A completed response can be reused across multiple separate comparisons.

For example, one person may compare their response with a team lead, teammate, mentor, and recruiter without answering the questionnaire again each time.

Each comparison remains separate. Reuse is only allowed after the user explicitly confirms that they want to use an existing response in the new comparison context.

### Use case 4: Invited participant gets their own private result

An invited participant is not just filling in the questionnaire for someone else.

After completing the questionnaire, the invited participant also receives:

- their own private reflection page
- their own private result link
- their own access code
- access to the comparison report when it is ready

### Use case 5: Group comparison

A small group can compare shared and different working preferences in one group report.

For example, a manager and several teammates may join the same comparison session.

The group report may show:

- where most participants have similar preferences
- where the group is split
- where there are different but complementary working styles
- conversation prompts for team alignment

This is a later feature, but the data model should support it from the beginning.

### Use case 6: Delete personal response and comparison access

A participant can delete their own response.

When a response is deleted:

- the participant’s private reflection page is no longer available
- the response can no longer be reused in new comparisons
- access through the private result link or access code is disabled
- comparisons that depend on this response are either removed, anonymized, or marked as unavailable

The app should make the deletion behavior clear before the user confirms deletion.

For MVP, deleting a response should remove the user’s answers and disable future access to related personal results. Audit events may keep a minimal record that deletion happened, but must not store raw answers.

## 7. Main user flows
### First participant flow
User opens app<br>
→ reads privacy/consent explanation<br>
→ fills in questionnaire<br>
→ receives personal reflection<br>
→ receives private result link<br>
→ receives access code<br>
→ creates invite link<br>
→ shares only invite link with another person<br>
### Invited participant flow
Invited person opens /invite/...<br>
→ reads explanation of what will be shared<br>
→ chooses one option:

   1. Fill in questionnaire
   2. Use existing access code
   3. Use existing private result link

→ gives explicit consent to join comparison<br>
→ receives their own private result link<br>
→ receives their own access code<br>
→ sees personal reflection<br>
→ sees comparison when ready<br>
### Reuse flow
Invited person opens invite<br>
→ app asks: “Already completed this questionnaire?”<br>
→ person enters access code<br>
→ app finds existing response<br>
→ app asks: “Use this response for this comparison?”<br>
→ person confirms<br>
→ response is attached to comparison<br>
→ no second questionnaire needed<br>

## 8. Key product rules and constraints
### Rule 1: Invite link is only for inviting

The link shared with another person should be an invite link:

/invite/{inviteToken}

It should not expose the inviter’s private result page.

### Rule 2: Each participant gets their own private result link

After completing the questionnaire, every participant receives their own private result link:

/me/{privateResultToken}

This page shows:

- personal reflection
- existing comparisons
- pending comparisons
- ability to create new comparisons
- access code reminder or regeneration option
- delete response option
### Rule 3: A response can be reused

A person should not need to answer the same questionnaire again if they already completed it.

Core model:

ResponseSet = one person’s completed answers to one questionnaire version
ComparisonSession = a comparison that references two or more ResponseSets

The same ResponseSet can be used in multiple comparisons.

### Rule 4: Reuse requires explicit consent

Even if the app recognizes an existing response, it must not automatically attach it to a new comparison.

The user must confirm:

Use this existing response for comparison with [Name]?
### Rule 5: Access code supports accountless reuse

Because the MVP has no accounts, users can reuse previous answers by entering an access code.

Example:

K7Q9-MP2D-W4T8

The access code should be shown after questionnaire completion and explained clearly.

### Rule 6: Access code must be private

The UI should warn:

Keep this access code private. Anyone with this code may be able to access or reuse your response.

Technically, the app should store only a hash of the access code, not the plain code.

### Rule 7: Raw answers are not shown by default

Comparison reports should show summaries, overlaps, differences, and prompts.

Raw answers should not be visible unless a future feature explicitly allows sharing them with consent.

### Rule 8: No compatibility score

The app should avoid:

87% compatible
good fit
bad fit
high empathy
low empathy
hire
reject

Instead, it should say things like:

You both prefer explicit expectations.
You differ in how much flexibility you prefer under deadline pressure.
This may be useful to discuss before working together.
### Rule 9: Results are explainable

Each insight should be traceable to questionnaire dimensions or specific questions.

Example:

Based on your answers about feedback rhythm and decision-making, you both prefer written context before important discussions.
### Rule 10: Context matters

A response may depend on context. Someone may answer differently for:

- job interview
- current team
- friendship
- cofounder relationship
- manager relationship

So reuse should include options:

Use existing response
Review/edit a copy
Fill in a new response

For MVP: 
Use existing response
Fill in again

and document “review/edit copy” as future improvement.

## 9. Privacy and ethics principles
This application is a reflective comparison tool. It does not diagnose personality, empathy, morality, emotional intelligence, mental health, or relationship compatibility. Results are based only on self-reported answers and are presented as neutral conversation prompts.

The app should be built around these principles:

- no psychological diagnosis
- no ranking people
- no emotional intelligence scoring
- no hidden sharing
- no raw answer sharing by default
- explicit consent before reuse
- private result links
- access codes treated as sensitive
- ability to delete response
- invite links expire
- minimal data collection

## 10. MVP scope
The first version should include:

- questionnaire definition
- questionnaire completion
- personal reflection result
- private result link
- access code generation
- invite link generation
- one-to-one comparison
- reuse existing response by access code
- explicit consent before reuse
- comparison result page
- delete response
- invite expiry
- audit event logging

The MVP should not include:

- user accounts
- social network profiles
- public search
- chat
- AI-generated psychological analysis
- employer ranking
- real hiring recommendations
- complex admin dashboard
- microservices

## 11. Future ideas

Later versions could add:

- optional accounts
- email magic links
- group comparisons
- PDF or Markdown report export
- questionnaire version management UI
- organization/team spaces
- optional raw answer sharing with consent
- analytics for small teams
- reporting module

## 12. Technical implications
The product should be designed as a modular monolith.

Possible modules:

Questionnaires
Responses
Comparisons
Reporting
Privacy
Audit
Notifications

Important domain objects:

QuestionnaireTemplate
QuestionnaireVersion
Question
AnswerOption
ScoringRule
InsightTemplate
ResponseSet
Answer
ComparisonSession
ComparisonParticipant
ComparisonResult
InviteToken
AccessCode
AuditEvent

Important technical decisions:

- deterministic comparison engine
- no LLM-generated interpretation in MVP
- access code stored as hash
- private tokens stored as hash
- comparison results tied to questionnaire version
- audit events record important lifecycle actions
- architecture tests enforce module boundaries

## 13. Audit events

The app should log important audit events for traceability, but not raw answers.

Events:

questionnaire_started
questionnaire_completed
personal_reflection_generated
comparison_invite_created
comparison_invite_opened
comparison_joined
existing_response_reuse_requested
existing_response_reuse_approved
comparison_generated
response_deleted
comparison_deleted
access_denied
invite_expired

Audit logs should answer:

Was consent given?
Was a response reused?
Was a comparison generated?
Was an invite expired?
Was a response deleted?
Was access denied?

They should not contain:

raw answers
private tokens
access codes
full result text
sensitive personal content