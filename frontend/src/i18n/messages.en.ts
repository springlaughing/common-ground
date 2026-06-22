import type { Messages } from './messages'

export const en: Messages = {
  language: {
    label: 'Language',
    en: 'English',
    de: 'Deutsch',
    changed: language => `Language set to ${language}. Your answers are kept.`,
  },
  welcome: {
    eyebrow: 'A working-style reflection',
    headline: 'Find your common ground.',
    lede:
      "A few questions about how you collaborate, plan, give feedback, and handle " +
      "pressure. It takes about ten minutes. There's no test, no score, and no right " +
      'answer — just a private reflection of how you work.',
    start: 'Get started →',
  },
  consent: {
    heading: 'Before we begin',
    collectTitle: 'What we collect',
    collectText: 'Only your answers to the questionnaire. No name, no email, no account.',
    useTitle: "How it's used",
    useText:
      'To generate a private reflection of your working style. Your individual answers stay private.',
    privateTitle: 'Private by design',
    privateText:
      "You'll get a private link to return to your results, and a separate access code " +
      'to reuse your response later. Both are yours alone.',
    agree: 'I understand what this is, and I want to begin.',
    begin: 'Begin →',
  },
  question: {
    previous: '← Previous',
    next: 'Next',
    submit: 'Submit',
    submitting: 'Sending…',
    answerOptions: 'Answer options',
    primaryChoice: ' — your primary choice',
    secondaryChoice: ' — your secondary choice',
    hintPickSecond: 'You can optionally pick a second preference. ',
    hintUndo: 'Tap a selected answer again to undo it.',
  },
  progress: {
    questionCounter: (current, total) => `Question ${current} / ${total}`,
    questionAria: (current, total) => `Question ${current} of ${total}`,
    sectionLabel: (current, total) => `Section ${current} of ${total}`,
  },
  completion: {
    eyebrow: 'All done',
    heading: 'Your reflection is ready.',
    lede:
      "Save these two before you go — they're the only way back to your results. We don't " +
      'store anything that could recover them for you.',
    view: 'View my reflection →',
  },
  credentials: {
    linkLabel: 'Your private result link',
    linkExplain: 'Bookmark this to return to your reflection anytime.',
    codeLabel: 'Your access code',
    codeExplain: 'Use this to reuse your response in a future comparison — not to open your results.',
    warning: 'Keep it private. Anyone who has it can reuse your response.',
    copy: 'Copy',
    copied: 'Copied',
    copyLinkAria: 'Copy private result link',
    copyCodeAria: 'Copy access code',
  },
  reflection: {
    eyebrow: 'Your reflection',
    title: 'How you work',
    intro:
      'Based on your answers. These are observations about how you tend to work — not ' +
      'scores, not a rating, and nothing here is better or worse than its opposite.',
    footnote:
      "Only the patterns your answers signal clearly are shown here — that's why some " +
      'themes may not appear.',
    compare: 'Compare with someone →',
  },
  shell: {
    back: '← Back',
  },
  status: {
    loading: 'Loading questionnaire…',
    loadFailed: 'Failed to load the questionnaire.',
    retry: 'Retry',
    submitFailed: 'Something went wrong submitting your answers. Please try again.',
  },
  me: {
    loading: 'Loading your reflection…',
    unavailableTitle: 'Result not available',
    unavailableBody:
      'This result link isn’t valid, or the result has been deleted. If you saved a ' +
      'private result link, double-check you copied the whole thing.',
    errorMessage: 'Something went wrong loading your reflection.',
    retry: 'Retry',
  },
  invite: {
    eyebrow: 'A comparison invite',
    title: 'You’ve been invited to compare working styles.',
    intro:
      'Answer the same short reflection, then see where you and the person who invited ' +
      'you align and where you differ.',
    loading: 'Checking your invite…',
    invalidTitle: 'Invite not available',
    invalidBody:
      'This invite link isn’t valid, has already been used, or has expired. Ask for a ' +
      'fresh link if you’d still like to compare.',
    joinedTitle: 'You’re in',
    joinedIntro:
      'Save your own private link and access code below — they’re how you return to your ' +
      'results and your comparison.',
    declinedTitle: 'No problem',
    declinedBody: 'You’ve declined this invite. Nothing was created and nothing was shared.',
    joinError: 'Something went wrong joining the comparison. Please try again.',
  },
  comparison: {
    eyebrow: 'Your comparison',
    title: 'Where you align and where you differ',
    back: '← Back',
  },
  consentInvitee: {
    heading: 'Before you join this comparison',
    intro: inviterLabel => `${inviterLabel} has invited you to compare working styles.`,
    whatTitle: 'What you’ll do',
    whatText:
      'You’ll answer the same short reflection they did. It takes about ten minutes and there ' +
      'are no right answers.',
    withWhomTitle: 'Who sees it',
    withWhomText: inviterLabel =>
      `Only you and ${inviterLabel} can see the comparison. Each of you opens it from your own ` +
      'private link — no accounts, and no one else has access.',
    whyTitle: 'What you’ll get',
    whyText:
      'A side-by-side view of where your working styles align and where they differ — described ' +
      'neutrally, with no score and no “better” or “worse”.',
    shareTitle: 'The label you choose',
    shareText: inviterLabel =>
      `The name or label you enter below is shown to ${inviterLabel} so they know who joined. ` +
      'Your individual answers are never shared.',
    labelLabel: 'Your name or label',
    labelPlaceholder: 'e.g. Sam',
    accept: 'Yes, I’ll join',
    decline: 'No, thanks',
  },
  inviteCreate: {
    heading: 'Invite someone to compare',
    intro:
      'Share a private, single-use link. They answer the same short reflection, then you ' +
      'each see where you align and where you differ. Your own answers stay private.',
    labelLabel: 'Your name or label',
    labelHint: 'Shown to the person you invite, so they know who this is from.',
    labelPlaceholder: 'e.g. Alex',
    create: 'Create invite link',
    creating: 'Creating…',
    cancel: 'Cancel',
    linkTitle: 'Your invite link is ready',
    linkNote: 'This link can be used once. Send it to the person you’d like to compare with.',
    copy: 'Copy link',
    copied: 'Copied',
    copyAria: 'Copy the invite link',
    error: 'Something went wrong creating your invite. Please try again.',
  },
}
