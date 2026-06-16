import { PageShell } from '../PageShell/PageShell'
import { useMessages } from '../../i18n/useMessages'
import styles from './WelcomeStep.module.css'

interface Props {
  onStart: () => void
}

export function WelcomeStep({ onStart }: Readonly<Props>) {
  const m = useMessages()

  return (
    <PageShell decoVariant="hero" showBrand={false} showLanguageSwitcher>
      <p className={styles.eyebrow}>{m.welcome.eyebrow}</p>

      <h1 className={styles.headline}>{m.welcome.headline}</h1>

      <p className={styles.lede}>{m.welcome.lede}</p>

      <div>
        <button className={styles.cta} onClick={onStart}>
          {m.welcome.start}
        </button>
      </div>
    </PageShell>
  )
}
