import { useState } from 'react'
import { PageShell } from '../PageShell/PageShell'
import styles from './ConsentStep.module.css'

interface Props {
  onAcknowledge: () => void
  onBack: () => void
}

const POINTS = [
  {
    title: 'What we collect',
    text: 'Only your answers to the questionnaire. No name, no email, no account.',
  },
  {
    title: "How it's used",
    text: 'To generate a private reflection of your working style. Your raw answers are never shown to anyone or put into reports.',
  },
  {
    title: 'Private by design',
    text: "You'll get a private link to return to your results, and a separate access code to reuse your response later. Both are yours alone.",
  },
]

export function ConsentStep({ onAcknowledge, onBack }: Props) {
  const [agreed, setAgreed] = useState(false)

  return (
    <PageShell onBack={onBack} decoVariant="hero" decoStyle="outline">
      <div className={styles.headingBlock}>
        <h1 className={styles.heading}>Before we begin</h1>
        <div className={styles.rule} />
      </div>

      <ul className={styles.points}>
        {POINTS.map(point => (
          <li key={point.title} className={styles.point}>
            <span className={styles.pointTitle}>{point.title}</span>
            <span className={styles.pointText}>{point.text}</span>
          </li>
        ))}
      </ul>

      <label className={styles.consent}>
        <input
          type="checkbox"
          className={styles.checkbox}
          checked={agreed}
          onChange={e => setAgreed(e.target.checked)}
        />
        <span>I understand what this is, and I want to begin.</span>
      </label>

      <div>
        <button className={styles.cta} onClick={onAcknowledge} disabled={!agreed}>
          Begin →
        </button>
      </div>
    </PageShell>
  )
}
