import type { Messages } from './messages'

export const de: Messages = {
  language: {
    label: 'Sprache',
    en: 'English',
    de: 'Deutsch',
    changed: language => `Sprache auf ${language} umgestellt. Deine Antworten bleiben erhalten.`,
  },
  welcome: {
    eyebrow: 'Eine Reflexion über deinen Arbeitsstil',
    headline: 'Findet heraus, wo ihr ähnlich tickt.',
    lede:
      'Ein paar Fragen dazu, wie du zusammenarbeitest, planst, Feedback gibst und mit Druck ' +
      'umgehst. Es dauert etwa zehn Minuten. Es gibt keinen Test, keine Punktzahl und keine ' +
      'richtige Antwort — nur eine private Reflexion darüber, wie du arbeitest.',
    start: "Los geht's →",
  },
  consent: {
    heading: 'Bevor wir beginnen',
    collectTitle: 'Was wir erfassen',
    collectText: 'Nur deine Antworten auf den Fragebogen. Kein Name, keine E-Mail, kein Account.',
    useTitle: 'Wofür wir es verwenden',
    useText:
      'Um eine private Reflexion über deinen Arbeitsstil zu erstellen. Deine einzelnen ' +
      'Antworten bleiben privat.',
    privateTitle: 'Von Anfang an privat',
    privateText:
      'Du erhältst einen privaten Link, mit dem du zu deinem Ergebnis zurückkehren kannst, ' +
      'und einen separaten Zugangscode, mit dem du deine Antworten später wiederverwenden ' +
      'kannst. Beides gehört nur dir.',
    agree: 'Ich bin einverstanden und möchte beginnen.',
    begin: 'Beginnen →',
  },
  question: {
    previous: '← Zurück',
    next: 'Weiter',
    submit: 'Absenden',
    submitting: 'Wird gesendet…',
    answerOptions: 'Antwortoptionen',
    primaryChoice: ' — deine erste Wahl',
    secondaryChoice: ' — deine zweite Wahl',
    hintPickSecond: 'Du kannst optional eine zweite Präferenz wählen. ',
    hintUndo: 'Tippe eine ausgewählte Antwort erneut an, um sie aufzuheben.',
  },
  progress: {
    questionCounter: (current, total) => `Frage ${current} / ${total}`,
    questionAria: (current, total) => `Frage ${current} von ${total}`,
    sectionLabel: (current, total) => `Abschnitt ${current} von ${total}`,
  },
  completion: {
    eyebrow: 'Geschafft',
    heading: 'Deine Reflexion ist fertig.',
    lede:
      'Sichere diese beiden, bevor du gehst — sie sind der einzige Weg zurück zu deinem ' +
      'Ergebnis. Wir speichern nichts, womit wir sie für dich wiederherstellen könnten.',
    view: 'Meine Reflexion ansehen →',
  },
  credentials: {
    linkLabel: 'Dein privater Ergebnislink',
    linkExplain: 'Setze ein Lesezeichen, um jederzeit zu deiner Reflexion zurückzukehren.',
    codeLabel: 'Dein Zugangscode',
    codeExplain:
      'Damit kannst du deine Antworten in einem späteren Vergleich wiederverwenden — nicht, ' +
      'um dein Ergebnis zu öffnen.',
    warning: 'Halte ihn geheim. Wer ihn hat, kann deine Antworten wiederverwenden.',
    copy: 'Kopieren',
    copied: 'Kopiert',
    copyLinkAria: 'Privaten Ergebnislink kopieren',
    copyCodeAria: 'Zugangscode kopieren',
  },
  reflection: {
    eyebrow: 'Dein Ergebnis',
    title: 'Wie du arbeitest',
    intro:
      'Deine Antworten zeichnen ein Bild davon, wie du arbeitest — ohne Punktzahl, ohne ' +
      'Bewertung. Es geht nicht um richtig oder falsch, sondern darum, was zu dir passt.',
    footnote:
      'Hier werden nur die Muster gezeigt, die deine Antworten klar erkennen lassen — ' +
      'deshalb tauchen manche Themen vielleicht nicht auf.',
    compare: 'Mit jemandem vergleichen →',
  },
  shell: {
    back: '← Zurück',
  },
  status: {
    loading: 'Fragebogen wird geladen…',
    loadFailed: 'Der Fragebogen konnte nicht geladen werden.',
    retry: 'Erneut versuchen',
    submitFailed: 'Beim Senden deiner Antworten ist etwas schiefgelaufen. Bitte versuche es erneut.',
  },
  me: {
    loading: 'Deine Reflexion wird geladen…',
    unavailableTitle: 'Ergebnis nicht verfügbar',
    unavailableBody:
      'Dieser Ergebnislink ist ungültig, oder das Ergebnis wurde gelöscht. Falls du einen ' +
      'privaten Ergebnislink gespeichert hast, prüfe, ob du ihn vollständig kopiert hast.',
    errorMessage: 'Beim Laden deiner Reflexion ist etwas schiefgelaufen.',
    retry: 'Erneut versuchen',
  },
  invite: {
    eyebrow: 'Eine Einladung zum Vergleich',
    title: 'Du wurdest eingeladen, Arbeitsstile zu vergleichen.',
    intro:
      'Beantworte dieselbe kurze Reflexion und sieh dann, wo du und die einladende Person ' +
      'ähnlich tickt und wo ihr euch unterscheidet.',
    loading: 'Deine Einladung wird geprüft…',
    invalidTitle: 'Einladung nicht verfügbar',
    invalidBody:
      'Dieser Einladungslink ist ungültig, wurde bereits verwendet oder ist abgelaufen. ' +
      'Bitte um einen neuen Link, wenn du dich trotzdem vergleichen möchtest.',
  },
  comparison: {
    eyebrow: 'Euer Vergleich',
    title: 'Wo ihr ähnlich tickt und wo nicht',
    back: '← Zurück',
  },
  consentInvitee: {
    heading: 'Bevor du diesem Vergleich beitrittst',
  },
  inviteCreate: {
    heading: 'Lade jemanden zum Vergleich ein',
    intro:
      'Teile einen privaten Link, der nur einmal funktioniert. Die andere Person beantwortet ' +
      'dieselbe kurze Reflexion, danach seht ihr beide, wo ihr ähnlich tickt und wo ihr euch ' +
      'unterscheidet. Deine eigenen Antworten bleiben privat.',
    labelLabel: 'Dein Name oder Label',
    labelHint: 'Wird der eingeladenen Person angezeigt, damit sie weiß, von wem die Einladung kommt.',
    labelPlaceholder: 'z. B. Alex',
    create: 'Einladungslink erstellen',
    creating: 'Wird erstellt…',
    cancel: 'Abbrechen',
    linkTitle: 'Dein Einladungslink ist fertig',
    linkNote: 'Dieser Link funktioniert nur einmal. Schicke ihn der Person, mit der du dich vergleichen möchtest.',
    copy: 'Link kopieren',
    copied: 'Kopiert',
    copyAria: 'Einladungslink kopieren',
    error: 'Beim Erstellen deiner Einladung ist etwas schiefgelaufen. Bitte versuche es erneut.',
  },
}
