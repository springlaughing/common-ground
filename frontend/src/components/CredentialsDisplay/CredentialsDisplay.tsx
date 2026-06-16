import { useState } from 'react'
import { useMessages } from '../../i18n/useMessages'
import styles from './CredentialsDisplay.module.css'

interface Props {
  /** Plain private result link, e.g. "/me#TOKEN" — shown once for bookmarking. */
  privateResultLink: string
  /** Plain access code, e.g. "K7Q9-MP2D-W4T8". */
  accessCode: string
}

function CopyButton({ value, ariaLabel }: Readonly<{ value: string; ariaLabel: string }>) {
  const [copied, setCopied] = useState(false)
  const m = useMessages()

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
    <button type="button" className={styles.copyBtn} onClick={copy} aria-label={ariaLabel}>
      {copied ? m.credentials.copied : m.credentials.copy}
    </button>
  )
}

export function CredentialsDisplay({ privateResultLink, accessCode }: Readonly<Props>) {
  const m = useMessages()
  const fullLink = 'window' in globalThis
    ? `${globalThis.location.origin}${privateResultLink}`
    : privateResultLink

  return (
    <div className={styles.root}>
      {/* Private result link */}
      <section className={styles.card}>
        <span className={styles.label}>{m.credentials.linkLabel}</span>
        <p className={styles.explain}>{m.credentials.linkExplain}</p>
        <div className={styles.valueRow}>
          <code className={styles.value}>{fullLink}</code>
          <CopyButton value={fullLink} ariaLabel={m.credentials.copyLinkAria} />
        </div>
      </section>

      {/* Access code */}
      <section className={styles.card}>
        <span className={styles.label}>{m.credentials.codeLabel}</span>
        <p className={styles.explain}>{m.credentials.codeExplain}</p>
        <div className={styles.valueRow}>
          <code className={`${styles.value} ${styles.code}`}>{accessCode}</code>
          <CopyButton value={accessCode} ariaLabel={m.credentials.copyCodeAria} />
        </div>
        <p className={styles.warning}>{m.credentials.warning}</p>
      </section>
    </div>
  )
}
