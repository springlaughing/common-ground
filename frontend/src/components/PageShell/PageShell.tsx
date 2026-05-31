import type { ReactNode } from 'react'
import styles from './PageShell.module.css'

interface Props {
  onBack?: () => void
  backLabel?: string
  children: ReactNode
}

/**
 * Shared screen frame: brand header (with optional back link), centered content,
 * and the decorative lowercase c / g motif. Used by the welcome and consent screens.
 */
export function PageShell({ onBack, backLabel = '← Back', children }: Props) {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        {onBack ? (
          <button className={styles.backLink} onClick={onBack}>
            {backLabel}
          </button>
        ) : (
          <span />
        )}
        <span className={styles.brand}>common ground</span>
      </header>

      <main className={styles.content}>{children}</main>

      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>
    </div>
  )
}
