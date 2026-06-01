import { PageShell } from '../PageShell/PageShell'
import { CredentialsDisplay } from '../CredentialsDisplay/CredentialsDisplay'
import styles from './CompletionStep.module.css'

interface Props {
  privateResultLink: string
  accessCode: string
  onViewReflection: () => void
}

export function CompletionStep({ privateResultLink, accessCode, onViewReflection }: Props) {
  return (
    <PageShell align="top">
      <p className={styles.eyebrow}>All done</p>
      <h1 className={styles.heading}>Your reflection is ready.</h1>
      <p className={styles.lede}>
        Save these two before you go — they're the only way back to your results. We don't
        store anything that could recover them for you.
      </p>

      <CredentialsDisplay privateResultLink={privateResultLink} accessCode={accessCode} />

      <div>
        <button className={styles.cta} onClick={onViewReflection}>
          View my reflection →
        </button>
      </div>
    </PageShell>
  )
}
