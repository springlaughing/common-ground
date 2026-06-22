/**
 * Shape of a UI-chrome message catalog. English and German catalogs both satisfy this
 * interface, so TypeScript strict mode enforces that every key exists in every language.
 * Interpolated strings (counters, aria labels) are functions so each locale controls word
 * order. Localized content (questions, insights, titles) comes from the API — only chrome
 * lives here.
 */
export interface Messages {
  /** Language switcher. Language names are endonyms (shown the same in every locale). */
  language: {
    label: string
    en: string
    de: string
    /** Live-region announcement spoken to assistive tech after the locale changes. */
    changed: (language: string) => string
  }
  welcome: {
    eyebrow: string
    headline: string
    lede: string
    start: string
  }
  consent: {
    heading: string
    collectTitle: string
    collectText: string
    useTitle: string
    useText: string
    privateTitle: string
    privateText: string
    agree: string
    begin: string
  }
  question: {
    previous: string
    next: string
    submit: string
    /** Footer button label while the final submit is in flight. */
    submitting: string
    answerOptions: string
    primaryChoice: string
    secondaryChoice: string
    hintPickSecond: string
    hintUndo: string
  }
  progress: {
    /** Visible counter, e.g. "Question 3 / 46". */
    questionCounter: (current: number, total: number) => string
    /** Screen-reader label, e.g. "Question 3 of 46". */
    questionAria: (current: number, total: number) => string
    /** Section label, used for both the visible text and the aria-label. */
    sectionLabel: (current: number, total: number) => string
  }
  completion: {
    eyebrow: string
    heading: string
    lede: string
    view: string
  }
  credentials: {
    linkLabel: string
    linkExplain: string
    codeLabel: string
    codeExplain: string
    warning: string
    copy: string
    copied: string
    copyLinkAria: string
    copyCodeAria: string
  }
  reflection: {
    eyebrow: string
    title: string
    intro: string
    footnote: string
    compare: string
  }
  shell: {
    back: string
  }
  status: {
    loading: string
    loadFailed: string
    retry: string
    submitFailed: string
  }
  /** The saved-reflection (/me) page: load/empty/error states a returning viewer may see. */
  me: {
    loading: string
    unavailableTitle: string
    unavailableBody: string
    errorMessage: string
    retry: string
  }
  /**
   * Invite flow chrome (the invitee lands on `/invite#TOKEN`). Only structural chrome
   * plus load/invalid states live here now; the invite-create UI (T017/T019) and the
   * full join page (T026) grow this group.
   */
  invite: {
    eyebrow: string
    title: string
    intro: string
    loading: string
    invalidTitle: string
    invalidBody: string
    /** Shown with the invitee's own credentials once they've joined. */
    joinedTitle: string
    joinedIntro: string
    /** Shown after the invitee declines — nothing was created or shared. */
    declinedTitle: string
    declinedBody: string
    /** Surfaced if the join request itself fails. */
    joinError: string
  }
  /**
   * Comparison report shell. The per-dimension insight text comes from the API (feature
   * 002's localized snippets); only page chrome lives here. Grown when the report is
   * wired to the API (T040/T042).
   */
  comparison: {
    eyebrow: string
    title: string
    back: string
  }
  /**
   * Invitee consent variant — distinct from the questionnaire `consent` above. §V-reviewed:
   * specific copy (what / with whom / why), the self-label disclosure, and equal-weight
   * accept/decline with no double negatives or urgency.
   */
  consentInvitee: {
    heading: string
    /** "{inviterLabel} has invited you to compare working styles." */
    intro: (inviterLabel: string) => string
    whatTitle: string
    whatText: string
    withWhomTitle: string
    withWhomText: (inviterLabel: string) => string
    whyTitle: string
    whyText: string
    shareTitle: string
    /** Discloses that the invitee's chosen label is shown to the inviter. */
    shareText: (inviterLabel: string) => string
    labelLabel: string
    labelPlaceholder: string
    /** Equal-weight, affirmative choices (no double negatives). */
    accept: string
    decline: string
  }
  /**
   * Invite-create panel (US1) — the inviter labels themselves and mints a single-use,
   * time-limited link from their own `/me` reflection.
   */
  inviteCreate: {
    heading: string
    intro: string
    labelLabel: string
    labelHint: string
    labelPlaceholder: string
    create: string
    /** Button label while the create request is in flight. */
    creating: string
    cancel: string
    linkTitle: string
    linkNote: string
    copy: string
    copied: string
    copyAria: string
    error: string
  }
}
