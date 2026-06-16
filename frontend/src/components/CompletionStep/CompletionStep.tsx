import { PageShell } from '../PageShell/PageShell'
import { CredentialsDisplay } from '../CredentialsDisplay/CredentialsDisplay'
import { useMessages } from '../../i18n/useMessages'
import styles from './CompletionStep.module.css'

interface Props {
  privateResultLink: string
  accessCode: string
  onViewReflection: () => void
  onBack?: () => void
}

export function CompletionStep({ privateResultLink, accessCode, onViewReflection, onBack }: Readonly<Props>) {
  const m = useMessages()

  return (
    <PageShell align="top" decoStyle="outline" onBack={onBack} backLabel={m.shell.back}>
      <p className={styles.eyebrow}>{m.completion.eyebrow}</p>
      <h1 className={styles.heading}>{m.completion.heading}</h1>
      <p className={styles.lede}>{m.completion.lede}</p>

      <CredentialsDisplay privateResultLink={privateResultLink} accessCode={accessCode} />

      <div>
        <button className={styles.cta} onClick={onViewReflection}>
          {m.completion.view}
        </button>
      </div>
    </PageShell>
  )
}
