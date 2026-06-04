import type { ReactNode } from 'react'
import styles from './PageShell.module.css'

interface Props {
  onBack?: () => void
  backLabel?: string
  /** 'center' (default) vertically centers content; 'top' aligns to the top for scrolling pages. */
  align?: 'center' | 'top'
  /** 'default' subtle letters; 'hero' huge sans letters that frame the text (landing). */
  decoVariant?: 'default' | 'hero'
  /** 'filled' (default) solid accent colour; 'outline' contour-only for subtler inner pages. */
  decoStyle?: 'filled' | 'outline'
  showBrand?: boolean
  children: ReactNode
}

/**
 * Shared screen frame: brand header (with optional back link), content area,
 * and the decorative lowercase c / g motif. Used by the welcome, consent, and
 * completion screens.
 */
export function PageShell({
  onBack,
  backLabel = '← Back',
  align = 'center',
  decoVariant = 'default',
  decoStyle = 'filled',
  showBrand = true,
  children,
}: Readonly<Props>) {
  const outlineClass = decoStyle === 'outline' ? ` ${styles.outlineDeco}` : ''

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
        {showBrand && <span className={styles.brand}>common ground</span>}
      </header>

      <main className={`${styles.content} ${align === 'top' ? styles.alignTop : ''}`}>
        <div className={styles.inner}>
          {/* Hero letters live inside the text block so they anchor to it (not the
              viewport) and keep their relationship to the text at any size. */}
          {decoVariant === 'hero' && (
            <div
              className={`${styles.heroDeco}${outlineClass}`}
              aria-hidden="true"
            >
              <span className={styles.c}>c</span>
              <span className={styles.g}>g</span>
            </div>
          )}
          {children}
        </div>
      </main>

      {/* Subtle screens keep the viewport-corner deco. */}
      {decoVariant === 'default' && (
        <div
          className={`${styles.deco}${outlineClass}`}
          aria-hidden="true"
        >
          <span className={styles.c}>c</span>
          <span className={styles.g}>g</span>
        </div>
      )}
    </div>
  )
}
