import { PageShell } from '../PageShell/PageShell'
import styles from './WelcomeStep.module.css'

interface Props {
  onStart: () => void
}

export function WelcomeStep({ onStart }: Readonly<Props>) {
  return (
    <PageShell decoVariant="hero" showBrand={false}>
      <p className={styles.eyebrow}>A working-style reflection</p>

      <h1 className={styles.headline}>
        Find your
        <br />
        common ground.
      </h1>

      <p className={styles.lede}>
        A few questions about how you collaborate, plan, give feedback, and handle
        pressure. It takes about ten minutes. There's no test, no score, and no right
        answer — just a private reflection of how you work.
      </p>

      <div>
        <button className={styles.cta} onClick={onStart}>
          Get started →
        </button>
      </div>
    </PageShell>
  )
}
