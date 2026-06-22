import { useState } from 'react'
import { PageShell } from '../PageShell/PageShell'
import { useMessages } from '../../i18n/useMessages'
import styles from './InviteeConsentStep.module.css'

interface Props {
  /** The inviter's self-label, shown so the invitee knows who invited them and who will see their label. */
  inviterLabel: string
  /** Explicit consent: proceed to the questionnaire with the invitee's chosen label. */
  onAccept: (inviteeLabel: string) => void
  /** Explicit decline: nothing is created or shared. */
  onDecline: () => void
}

// Matches the backend label column length.
const MAX_LABEL_LENGTH = 60

/**
 * US2 (T027) — the invitee's consent screen, distinct from the first-time questionnaire consent.
 * §V-compliant: specific copy (what / who sees it / what they get / the label they share), the
 * self-label disclosure, and equal-weight affirmative accept/decline (no double negatives, no
 * urgency, decline as prominent as accept). Consent is the explicit Accept click.
 */
export function InviteeConsentStep({ inviterLabel, onAccept, onDecline }: Readonly<Props>) {
  const m = useMessages()
  const c = m.consentInvitee
  const [label, setLabel] = useState('')

  const points = [
    { title: c.whatTitle, text: c.whatText },
    { title: c.withWhomTitle, text: c.withWhomText(inviterLabel) },
    { title: c.whyTitle, text: c.whyText },
    { title: c.shareTitle, text: c.shareText(inviterLabel) },
  ]

  return (
    <PageShell showLanguageSwitcher decoVariant="hero" decoStyle="outline">
      <div className={styles.headingBlock}>
        <h1 className={styles.heading}>{c.heading}</h1>
        <p className={styles.intro}>{c.intro(inviterLabel)}</p>
        <div className={styles.rule} />
      </div>

      <ul className={styles.points}>
        {points.map(point => (
          <li key={point.title} className={styles.point}>
            <span className={styles.pointTitle}>{point.title}</span>
            <span className={styles.pointText}>{point.text}</span>
          </li>
        ))}
      </ul>

      <label className={styles.labelField}>
        <span className={styles.labelText}>{c.labelLabel}</span>
        <input
          type="text"
          className={styles.input}
          value={label}
          maxLength={MAX_LABEL_LENGTH}
          placeholder={c.labelPlaceholder}
          onChange={e => setLabel(e.target.value)}
        />
      </label>

      <div className={styles.actions}>
        <button type="button" className={styles.decline} onClick={onDecline}>
          {c.decline}
        </button>
        <button
          type="button"
          className={styles.accept}
          onClick={() => onAccept(label.trim())}
          disabled={label.trim().length === 0}
        >
          {c.accept}
        </button>
      </div>
    </PageShell>
  )
}
