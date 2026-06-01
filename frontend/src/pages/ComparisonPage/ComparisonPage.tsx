import type { ComparisonDto, ComparisonInsightDto } from '../../types/api'
import styles from './ComparisonPage.module.css'

interface Props {
  comparison: ComparisonDto
  onBack?: () => void
}

const isAligned = (i: ComparisonInsightDto) =>
  i.yourStrength !== null && i.theirStrength !== null && Math.abs(i.yourStrength - i.theirStrength) <= 1

function DotStrength({ label, strength, variant }: { label: string; strength: number | null; variant: 'you' | 'them' }) {
  return (
    <div className={styles.dotRow}>
      <span className={`${styles.dotLabel} ${variant === 'you' ? styles.dotLabelYou : styles.dotLabelThem}`}>
        {label}
      </span>
      <div className={styles.dots}>
        {strength !== null
          ? [1, 2, 3, 4, 5].map(i => (
              <span
                key={i}
                className={`${styles.dot} ${i <= strength ? (variant === 'you' ? styles.dotFilledYou : styles.dotFilledThem) : styles.dotEmpty}`}
              />
            ))
          : <span className={styles.dotAbsent}>—</span>
        }
      </div>
    </div>
  )
}

function InsightCard({ insight, theirName }: { insight: ComparisonInsightDto; theirName: string }) {
  const aligned = isAligned(insight)
  const bothScored = insight.yourStrength !== null && insight.theirStrength !== null
  // When both scored the same dimension, the texts describe the same concept in different pronouns.
  // Show one text and let the dot positions communicate the intensity difference.
  // Two labeled rows are only meaningful when one person scored and the other didn't.
  const showOneText = bothScored
  const sharedText = insight.yourText ?? insight.theirText

  return (
    <article className={styles.insight}>
      <h3 className={styles.insightTitle}>{insight.title}</h3>
      <div className={styles.dotStrengths}>
        <DotStrength label="You" strength={insight.yourStrength} variant="you" />
        <DotStrength label={theirName} strength={insight.theirStrength} variant="them" />
      </div>
      {aligned && <span className={styles.alignBadge}>● Similar intensity</span>}
      {showOneText ? (
        sharedText && <p className={styles.insightText}>{sharedText}</p>
      ) : (
        <div className={styles.insightRows}>
          <div className={styles.insightRow}>
            <span className={`${styles.insightLabel} ${styles.insightLabelYou}`}>● You</span>
            {insight.yourText
              ? <p className={styles.insightText}>{insight.yourText}</p>
              : <p className={styles.insightTextAbsent}>This didn't show strongly in your answers.</p>
            }
          </div>
          <div className={styles.insightRow}>
            <span className={`${styles.insightLabel} ${styles.insightLabelThem}`}>● {theirName}</span>
            {insight.theirText
              ? <p className={styles.insightText}>{insight.theirText}</p>
              : <p className={styles.insightTextAbsent}>This didn't show strongly in their answers.</p>
            }
          </div>
        </div>
      )}
    </article>
  )
}

export function ComparisonPage({ comparison, onBack }: Props) {
  const allInsights = comparison.groups.flatMap(g => g.insights)
  const alignedInsights = allInsights.filter(isAligned)
  const differsInsights = allInsights.filter(i => !isAligned(i))

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        {onBack ? (
          <button className={styles.backLink} onClick={onBack}>← Your reflection</button>
        ) : (
          <span />
        )}
        <span className={styles.brand}>common ground</span>
      </header>

      <main className={styles.content}>
        <p className={styles.eyebrow}>Comparison</p>
        <h1 className={styles.title}>How you work together.</h1>

        <div className={styles.legend}>
          <span className={styles.legendYou}>● You</span>
          <span className={styles.legendDash}>——</span>
          <span className={styles.legendThem}>● {comparison.theirName}</span>
        </div>

        <div className={styles.summary}>{comparison.summary}</div>

        {alignedInsights.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionHeader}>Where you're aligned</h2>
            <div className={styles.insightList}>
              {alignedInsights.map(i => (
                <InsightCard key={i.dimensionId} insight={i} theirName={comparison.theirName} />
              ))}
            </div>
          </section>
        )}

        {differsInsights.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionHeader}>Where you differ</h2>
            <div className={styles.insightList}>
              {differsInsights.map(i => (
                <InsightCard key={i.dimensionId} insight={i} theirName={comparison.theirName} />
              ))}
            </div>
          </section>
        )}
      </main>

      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>
    </div>
  )
}
