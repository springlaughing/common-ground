import { useState } from 'react'
import { PageShell } from '../PageShell/PageShell'
import { useMessages } from '../../i18n/useMessages'
import styles from './ConsentStep.module.css'

interface Props {
  onAcknowledge: () => void
  onBack: () => void
}

export function ConsentStep({ onAcknowledge, onBack }: Readonly<Props>) {
  const [agreed, setAgreed] = useState(false)
  const m = useMessages()

  const points = [
    { title: m.consent.collectTitle, text: m.consent.collectText },
    { title: m.consent.useTitle, text: m.consent.useText },
    { title: m.consent.privateTitle, text: m.consent.privateText },
  ]

  return (
    <PageShell onBack={onBack} backLabel={m.shell.back} showLanguageSwitcher decoVariant="hero" decoStyle="outline">
      <div className={styles.headingBlock}>
        <h1 className={styles.heading}>{m.consent.heading}</h1>
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

      <label className={styles.consent}>
        <input
          type="checkbox"
          className={styles.checkbox}
          checked={agreed}
          onChange={e => setAgreed(e.target.checked)}
        />
        <span>{m.consent.agree}</span>
      </label>

      <div>
        <button className={styles.cta} onClick={onAcknowledge} disabled={!agreed}>
          {m.consent.begin}
        </button>
      </div>
    </PageShell>
  )
}
