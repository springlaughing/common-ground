# Feature Specification: Questionnaire Completion

**Feature Branch**: `001-questionnaire-completion`

**Created**: 2026-05-14

**Status**: Draft

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Complete questionnaire and receive personal reflection (Priority: P1)

A new user opens the app for the first time. Before seeing any questions, they are shown a clear explanation of what the app does, what data will be collected, and how it will be used. The user acknowledges the consent explanation and proceeds to fill in the questionnaire. After submitting their answers, they receive a personal reflection page showing their working-style patterns, a private result link for future access, and an access code for reusing their response in future comparisons.

**Why this priority**: This is the entry point for every user. No other feature works without a completed response. It also establishes the privacy-first tone of the product from the very first interaction.

**Independent Test**: Can be fully tested by opening the app, completing the consent flow and questionnaire, and verifying that a personal reflection page, private result link, and access code are produced.

**Acceptance Scenarios**:

1. **Given** a user opens the app for the first time, **When** they land on the home page, **Then** they see a privacy and consent explanation before any questionnaire content is shown
2. **Given** a user is on the consent page, **When** they have not acknowledged the consent, **Then** they cannot proceed to the questionnaire
3. **Given** a user acknowledges the consent, **When** they proceed, **Then** the questionnaire is displayed with all questions
4. **Given** a user is filling in the questionnaire, **When** they attempt to submit with unanswered required questions, **Then** they are prompted to complete all required questions before submitting
5. **Given** a user completes and submits the questionnaire, **When** the submission is processed, **Then** they are shown a personal reflection page with their working-style patterns
6. **Given** a user completes the questionnaire, **When** the reflection page is shown, **Then** it also displays their private result link and access code
7. **Given** a user views their access code, **When** it is displayed, **Then** a clear warning is shown that the access code is private and should not be shared

---

### User Story 2 — View personal reflection via private result link (Priority: P2)

A user who has previously completed the questionnaire bookmarks their private result link. When they return to the app using that link, they can view their personal reflection page showing their working-style patterns.

**Why this priority**: The private result link is the user's only way to return to their results. Without it working correctly, the app has no continuity for the user.

**Independent Test**: Can be tested by completing the questionnaire, copying the private result link, navigating away, and returning via the link to verify the reflection page is accessible and correct.

**Acceptance Scenarios**:

1. **Given** a user has a valid private result link, **When** they navigate to it, **Then** they are shown their personal reflection page
2. **Given** a user navigates to a private result link, **When** the link is valid, **Then** the page shows the same working-style patterns as shown immediately after completion
3. **Given** a user navigates to an invalid or non-existent private result link, **When** the page loads, **Then** they see a clear message that the result is not available — not a technical error
4. **Given** a user is on their personal reflection page, **When** they view it, **Then** their access code is visible or can be revealed on demand

---

### User Story 3 — Understand credentials after completion (Priority: P2)

After completing the questionnaire, the user receives two separate credentials: a private result link and an access code. The app clearly explains the purpose of each and how they differ, so the user understands what to save and why.

**Why this priority**: If users do not understand what the private result link and access code are for, they will lose access to their results and be unable to reuse their response. The explanation at this step is critical.

**Independent Test**: Can be tested by completing the questionnaire and verifying that the credential display screen clearly distinguishes the two credentials and explains each one's purpose.

**Acceptance Scenarios**:

1. **Given** a user has just completed the questionnaire, **When** their credentials are shown, **Then** the private result link and access code are displayed separately with distinct labels
2. **Given** a user views their credentials, **When** the access code is shown, **Then** the explanation states it is for reusing their response in a future comparison — not for opening their result page
3. **Given** a user views their credentials, **When** the private result link is shown, **Then** the explanation states it is the link to bookmark for returning to their results
4. **Given** a user views their credentials, **When** the access code is displayed, **Then** a privacy warning is shown: keeping the access code private prevents others from reusing their response

---

### Edge Cases

- What happens if the user closes the browser mid-questionnaire? In the MVP, in-progress answers are not saved. The user must start again from the consent step.
- What happens if the same user completes the questionnaire twice? Each completion produces a new, independent response set with its own private result link and access code. The app does not detect or prevent this.
- What happens if a new questionnaire version is published after a user has completed an older version? The user's existing response remains valid for the version they completed. They cannot use it with invites that require the newer version.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST display a privacy and consent explanation to the user before showing any questionnaire content
- **FR-002**: The user MUST explicitly acknowledge the consent explanation before proceeding to the questionnaire
- **FR-003**: The questionnaire MUST present all questions from the active questionnaire version
- **FR-004**: All questions MUST be required — the user cannot submit with unanswered questions
- **FR-005**: The app MUST save the user's answers as a completed response upon submission
- **FR-006**: The app MUST generate a personal reflection from the completed response using the deterministic comparison engine
- **FR-007**: Each insight in the personal reflection MUST be neutral and descriptive — no overall compatibility score, no diagnosis, no ranking
- **FR-008**: The app MUST generate a unique private result link for the participant after completion
- **FR-009**: The app MUST generate a unique access code for the participant after completion
- **FR-010**: The private result link MUST give access to the personal reflection page when used
- **FR-011**: The access code MUST be displayed with a clear explanation of its purpose and a privacy warning
- **FR-012**: The personal reflection page MUST remain accessible via the private result link after the initial session ends
- **FR-013**: The app MUST log the following audit events: `questionnaire_started`, `questionnaire_completed`, `personal_reflection_generated`
- **FR-014**: Audit events MUST NOT contain raw answers, tokens, or access codes

#### Scoring Engine

- **FR-015**: The scoring engine MUST compute a raw dimension score for each dimension by summing weighted answer contributions: primary answer at ×1.0, optional secondary answer at ×0.5
- **FR-016**: Each raw dimension score MUST be normalised against that dimension's maximum achievable score, producing a normalised score in the range 0.0–1.0. Maximum achievable score per dimension is computed at seed time from the weight table.
- **FR-017**: The scoring engine MUST be deterministic — identical response inputs always produce identical normalised scores
- **FR-018**: Dimensions with a normalised score below the display threshold (default: 0.4) MUST NOT appear on the personal reflection page

#### Personal Reflection Page

- **FR-019**: The personal reflection page MUST organise visible dimensions into the 10 groups defined in `reflection-groups.json`, in group order
- **FR-020**: Groups where no dimension meets the display threshold MUST be omitted from the page entirely
- **FR-021**: Each visible dimension MUST be rendered using its pre-authored insight snippet from `reflection-groups.json`
- **FR-022**: Each visible dimension MUST display a 5-point visual strength indicator derived from its normalised score
- **FR-023**: The personal reflection page MUST NOT display raw dimension scores or numeric values to the user

### Key Entities

- **QuestionnaireVersion**: A versioned, immutable snapshot of the questionnaire. Contains questions, answer options, and scoring rules. Only one version is active at a time for MVP.
- **Question**: A single question within a questionnaire version. Has a defined dimension it measures.
- **AnswerOption**: A selectable answer for a question. Has a scoring value used by the comparison engine.
- **ResponseSet**: A participant's completed answers to one questionnaire version. Immutable once submitted.
- **Answer**: A single answer given by a participant to a specific question.
- **DimensionGroup**: A named group of related dimensions defined in `reflection-groups.json`. Has a human-readable title and an ordered list of dimension IDs. Used to organise the personal reflection page into thematic sections. There are 10 groups covering topics such as planning style, feedback, conflict, and motivation.
- **InsightSnippet**: A pre-authored second-person text for a specific dimension, stored in `reflection-groups.json`. Rendered on the personal reflection page when the dimension's normalised score meets the display threshold. One snippet per dimension — no LLM involvement at runtime.
- **PrivateResultCredential**: The participant's private result link credential. Stored as a hash. Used to access the personal reflection page.
- **AccessCode**: The participant's reuse credential. Stored as a hash. Used only to reuse a response in a future comparison.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can complete the full flow — consent, questionnaire, reflection — in under 15 minutes
- **SC-002**: 100% of submitted responses produce a personal reflection with at least one insight
- **SC-003**: A participant can return to their personal reflection page via their private result link at any time after completion
- **SC-004**: Every insight on the personal reflection page is traceable to a specific questionnaire dimension
- **SC-005**: No raw answers appear on the personal reflection page or in any audit log
- **SC-006**: The private result link and access code are always displayed with distinct labels and separate explanations
- **SC-007**: No raw dimension scores or numeric values appear on the personal reflection page
- **SC-008**: The personal reflection page omits any group where no dimension meets the display threshold — the page reflects only what the response actually signals about this person

---

## Assumptions

- The questionnaire content (questions, answer options, scoring rules, insight templates) is defined and seeded as part of this feature. Designing the questionnaire itself is in scope.
- Only one questionnaire version is active in the MVP. Version management UI is out of scope.
- In-progress questionnaire answers are not saved. Users who leave mid-flow must start again.
- The personal reflection shows working-style patterns for the individual user only. Comparison with another person's response is out of scope for this feature.
- The app has no user accounts. Each completion produces independent credentials with no link to prior completions.
- The questionnaire presents one question at a time with a progress indicator and next/back navigation. All questions live on a single page — there is no separate URL per question. The transition between questions is handled within the page using client-side state.
