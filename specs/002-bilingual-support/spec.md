# Feature Specification: Bilingual (English/German) Support

**Feature Branch**: `002-bilingual-support`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Bilingual (English/German) support for the app via an in-UI language switcher. One app, one questionnaire version, served in either English or German, switchable from a UI control. Scoring/comparison logic unchanged — only display text is localized. Content to localize: questions, answer options, dimension-group titles, dimension titles (net-new, currently unwired), insight texts, and UI chrome. Third-person comparison insights deferred."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Take the questionnaire and read the reflection in my language (Priority: P1)

A German-speaking person opens the app, selects German, completes the consent step and all questions in German, and receives their personal reflection — group headings, insight titles, and insight text — entirely in German. An English-speaking person gets the identical experience in English (the current behaviour, preserved).

**Why this priority**: This is the core value of the feature and a viable MVP on its own. Without an end-to-end localized journey, nothing else matters. It also exercises every category of localized content at least once.

**Independent Test**: Select German at the start, walk the full flow (consent → 45 questions → reflection), and confirm that 100% of visible text — instructions, questions, all answer options, buttons, group headings, insight titles, and insight paragraphs — is in German, with no English leaking through. Repeat in English.

**Acceptance Scenarios**:

1. **Given** a first-time visitor with no saved preference, **When** they open the app, **Then** the experience is presented in English by default and a language control is visible.
2. **Given** a visitor who selects German, **When** they proceed through consent and questions, **Then** every question and answer option appears in German.
3. **Given** a visitor who completed the questionnaire in German, **When** the reflection is shown, **Then** group headings, insight titles, and insight texts all appear in German.
4. **Given** the same set of answers submitted in English versus German, **When** each reflection is generated, **Then** the two reflections contain the same insights, in the same order, with the same strength values — differing only in display language.

---

### User Story 2 - Switch language at any time without losing progress (Priority: P2)

A person starts the questionnaire in English, answers several questions, then decides to continue in German (or vice versa). They change the language from the in-UI control and continue from exactly where they were, with their existing answers intact.

**Why this priority**: Real users change their minds or share a device. Losing answers on a language switch would be a serious usability failure. It is separable from US1 (US1 can ship with switch-at-start only) but materially improves the experience.

**Independent Test**: Answer the first few questions in English, switch to German mid-flow, and confirm: (a) previously selected answers are still selected, (b) the current and remaining questions/options now render in German, (c) the user's position in the flow is unchanged.

**Acceptance Scenarios**:

1. **Given** a user partway through the questionnaire with answers selected, **When** they switch language, **Then** all previously selected primary and secondary answers remain selected.
2. **Given** a user on question N, **When** they switch language, **Then** they remain on question N (no reset, no progress loss).
3. **Given** a user who switches language, **When** the page re-renders, **Then** the language control reflects the newly active language.

---

### User Story 3 - See a short title on every reflection insight, in my language (Priority: P2)

On the reflection page, each insight shows a short title above its strength dots and explanatory text — e.g. "Written records over memory" / "Schriftliches vor Gedächtnis" — in the active language. (Today the title slot exists in the layout but is empty because the backend never supplies a title.)

**Why this priority**: These titles are the scannable anchor on the reflection page and the row labels the upcoming comparison feature depends on. They are net-new plumbing (the title must be supplied for both languages, English included), so this story delivers value even to English users — who currently see no title at all.

**Independent Test**: Complete the questionnaire, open the reflection, and confirm every insight card shows a non-empty title in the active language, positioned above its strength dots and text, in both English and German.

**Acceptance Scenarios**:

1. **Given** a generated reflection in English, **When** it is displayed, **Then** every insight shows a non-empty English title.
2. **Given** a generated reflection in German, **When** it is displayed, **Then** every insight shows a non-empty German title.
3. **Given** an insight title is shown, **When** the card renders, **Then** its position and the surrounding layout match the existing design (title, then strength dots, then text) with no layout regression.

---

### User Story 4 - Open a saved reflection in either language (Priority: P3)

A returning user opens their private reflection link. The reflection renders in their currently selected language, and they can switch language while viewing it — a reflection completed in German can later be read in English, and vice versa.

**Why this priority**: Saved-reflection access is an existing capability; making it language-aware is valuable but lower-risk and lower-frequency than the primary completion journey.

**Independent Test**: Complete the questionnaire in German, open the saved reflection link in a fresh session, confirm it renders, switch to English, and confirm all content re-renders in English while showing the same insights/strengths.

**Acceptance Scenarios**:

1. **Given** a saved reflection and a user whose selected language is English, **When** they open the reflection link, **Then** the reflection renders in English.
2. **Given** a user viewing a saved reflection, **When** they switch language, **Then** the same insights, order, and strengths are shown, re-rendered in the newly selected language.
3. **Given** a reflection created before this feature shipped, **When** it is opened after release, **Then** it still renders (in English) and now shows insight titles.

---

### Edge Cases

- **Switch mid-questionnaire**: answers and position are preserved; only display text changes (covered by US2).
- **Missing translation for one item**: the item falls back to English rather than rendering empty or broken; the rest of the page stays in the chosen language.
- **Pre-existing (English-only) saved reflections**: continue to render after release, now with insight titles populated.
- **Longer German text**: German strings are typically longer than English; answer options and insight cards must remain readable without overflow or truncation.
- **Browser set to a language other than English or German**: the app defaults to English.
- **Assistive technology**: the document's active language is conveyed to screen readers so content is pronounced correctly, and a language change is reflected without a full reload breaking focus.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST present all user-facing content in English or German, selectable by the user, with English and German offering identical coverage.
- **FR-002**: System MUST provide a language switcher that is visible and operable (including by keyboard and assistive technology) on every primary page of the flow (consent/start, each question step, and the reflection page).
- **FR-003**: System MUST default to English when the user has no stored language preference.
- **FR-004**: System MUST retain the user's chosen language across page navigation and across return visits on the same device, without requiring an account.
- **FR-005**: When the user changes language at any point, the System MUST immediately present subsequent content in the chosen language while preserving all answers already provided and the user's current position in the flow.
- **FR-006**: System MUST present the same set of questions, answer options, dimension groups, dimensions, and insights in every supported language (parallel completeness — no language has missing or extra items).
- **FR-007**: System MUST localize questionnaire questions, answer-option texts, dimension-group titles, dimension titles, and reflection insight texts.
- **FR-008**: System MUST display a short, non-empty title on each reflection insight — positioned above its strength indicator and explanatory text — in the active language. This title MUST be available for both English and German (it is net-new; it does not exist end-to-end today).
- **FR-009**: System MUST localize all interface chrome, including the consent explanation, progress indicator, page headings/intros/footnotes, and action buttons.
- **FR-010**: The set of insights shown, their order, and their strength values for a given set of answers MUST be identical regardless of language; language MUST affect display text only, never scoring, selection, or ordering.
- **FR-011**: When a returning user opens a saved reflection link, the System MUST render it in the user's currently selected language and MUST allow switching language while viewing it.
- **FR-012**: System MUST convey the active content language to assistive technologies (correct document language indication) and update it when the language changes.
- **FR-013**: If a translation for a content item is missing in the active language, the System MUST fall back to English for that item rather than display an empty or broken element.
- **FR-014**: Localization MUST NOT cause raw questionnaire answer text to be logged or exposed; existing privacy guarantees are unchanged.
- **FR-015**: Adding a future language MUST require only supplying translated text — not changes to scoring rules, dimension identifiers, question structure, or insight selection logic.

### Key Entities *(include if feature involves data)*

- **Supported Language**: a language the app can present content in. The set is English and German for this version, with English as the default and fallback. Designed to allow additional languages later.
- **Localized Text (Translation)**: the text of a content item for a specific language, identified by the content item's stable identifier plus the language. Spans questions, answer options, dimension-group titles, dimension titles, and insight texts (and, conceptually, interface chrome strings).
- **Dimension Title**: a short label associated with a dimension, shown on the reflection insight above the strength indicator. Net-new content that must exist for each supported language.
- **Language Preference**: the user's currently chosen language, retained on their own device. It is accountless and is not stored server-side alongside the user's answers.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can complete the entire flow (consent → questionnaire → reflection) with 100% of visible text in their chosen language and zero untranslated strings, in both English and German.
- **SC-002**: Switching language at any step preserves 100% of previously entered answers and the user's exact position in the flow.
- **SC-003**: For an identical set of answers, the reflection presents the same insights, in the same order, with the same strength values in both languages — zero differences in structure or scoring.
- **SC-004**: Every reflection insight displays a non-empty title in the active language — zero empty title slots — for both English and German.
- **SC-005**: A returning user can open a saved reflection and view it in either language, switching between them, with content fully re-rendered each time and identical insights/strengths.
- **SC-006**: The language switcher is present and operable (mouse, keyboard, and screen reader) on 100% of the flow's primary pages.
- **SC-007**: After changing language, translated content is visible near-instantly (under ~1 second perceived) with no loss of the current page or answers.

## Assumptions

- English is the default and fallback language; automatic browser-language detection is out of scope for this version and can be added later without rework.
- Exactly two languages this version: English and German. The content model should accommodate additional languages later.
- German text for questions and answer options has already been drafted (`questionary_german.md`). German for dimension titles, dimension-group titles, insight texts, and interface chrome will be produced as part of this work.
- The 76 third-person ("They…") insight texts used only by the not-yet-built comparison feature are out of scope here.
- The user's language preference is stored on their device (accountless) and is not persisted server-side against their answers, consistent with privacy minimization.
- Saved reflections carry no recorded language; they are rendered in the viewer's currently selected language, so the same reflection can be viewed in either language.
- The deterministic scoring engine operates on stable identifiers and weights, not on display text, so localization does not change scores. (Constitution: Deterministic Engine, Neutral Outputs.)
- Localized content is seeded and served automatically on release (migrations run on deploy), requiring no manual data step.
- Translations are human-authored; machine translation at runtime is out of scope.

## Out of Scope

- Third-person ("They…") comparison insight texts (belongs to the future comparison feature).
- Languages other than English and German.
- Automatic/machine translation and runtime translation services.
- Right-to-left language support.
- Browser-language auto-detection (English default is used instead).
