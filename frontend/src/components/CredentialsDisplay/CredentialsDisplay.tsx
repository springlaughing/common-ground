import { useState } from 'react'
import styles from './CredentialsDisplay.module.css'

interface Props {
  /** Plain private result link, e.g. "/me#TOKEN" — shown once for bookmarking. */
  privateResultLink: string
  /** Plain access code, e.g. "K7Q9-MP2D-W4T8". */
  accessCode: string
}

function CopyButton({ value, label }: { value: string; label: string }) {
  const [copied, setCopied] = useState(false)

  async function copy() {
    try {
      await navigator.clipboard.writeText(value)
      setCopied(true)
      setTimeout(() => setCopied(false), 1600)
    } catch {
      // Clipboard may be unavailable (e.g. insecure context); fail quietly.
    }
  }

  return (
    <button type="button" className={styles.copyBtn} onClick={copy} aria-label={`Copy ${label}`}>
      {copied ? 'Copied' : 'Copy'}
    </button>
  )
}

export function CredentialsDisplay({ privateResultLink, accessCode }: Props) {
  const fullLink =
    typeof window !== 'undefined' ? `${window.location.origin}${privateResultLink}` : privateResultLink

  return (
    <div className={styles.root}>
      {/* Private result link */}
      <section className={styles.card}>
        <span className={styles.label}>Your private result link</span>
        <p className={styles.explain}>Bookmark this to return to your reflection anytime.</p>
        <div className={styles.valueRow}>
          <code className={styles.value}>{fullLink}</code>
          <CopyButton value={fullLink} label="private result link" />
        </div>
      </section>

      {/* Access code */}
      <section className={styles.card}>
        <span className={styles.label}>Your access code</span>
        <p className={styles.explain}>
          Use this to reuse your response in a future comparison — not to open your results.
        </p>
        <div className={styles.valueRow}>
          <code className={`${styles.value} ${styles.code}`}>{accessCode}</code>
          <CopyButton value={accessCode} label="access code" />
        </div>
        <p className={styles.warning}>
          Keep it private. Anyone who has it can reuse your response.
        </p>
      </section>
    </div>
  )
}
