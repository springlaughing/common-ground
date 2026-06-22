import type { ReactNode } from 'react'
import type { ComparisonDto, ComparisonInsightDto } from '../../types/api'
import { useMessages } from '../../i18n/useMessages'
import styles from './ComparisonPage.module.css'

interface Props {
  comparison: ComparisonDto
  onBack?: () => void
  /** Optional header-left slot (the /me hub mounts the language switcher so the report is
   *  switchable at view time — it re-fetches in the new locale). */
  languageSwitcher?: ReactNode
}

function DotStrength({ label, strength, variant }: Readonly<{ label: string; strength: number | null; variant: 'you' | 'them' }>) {
  const labelClass = variant === 'you' ? styles.dotLabelYou : styles.dotLabelThem
  const filledClass = variant === 'you' ? styles.dotFilledYou : styles.dotFilledThem
  return (
    <div className={styles.dotRow}>
      <span className={`${styles.dotLabel} ${labelClass}`}>{label}</span>
      <div className={styles.dots}>
        {strength === null
          ? <span className={styles.dotAbsent}>—</span>
          : [1, 2, 3, 4, 5].map(i => (
              <span key={i} className={`${styles.dot} ${i <= strength ? filledClass : styles.dotEmpty}`} />
            ))
        }
      </div>
    </div>
  )
}

function InsightCard({ insight, otherLabel }: Readonly<{ insight: ComparisonInsightDto; otherLabel: string }>) {
  const m = useMessages()
  const bothScored = insight.yourStrength !== null && insight.theirStrength !== null
  // When both scored the same dimension, the texts describe the same concept in different pronouns:
  // show one and let the dot positions convey the intensity difference. Two labeled rows are only
  // meaningful when one person scored and the other didn't.
  const sharedText = insight.yourText ?? insight.theirText

  return (
    <article className={styles.insight}>
      <h3 className={styles.insightTitle}>{insight.title}</h3>
      <div className={styles.dotStrengths}>
        <DotStrength label={m.comparison.you} strength={insight.yourStrength} variant="you" />
        <DotStrength label={otherLabel} strength={insight.theirStrength} variant="them" />
      </div>
      {bothScored ? (
        sharedText && <p className={styles.insightText}>{sharedText}</p>
      ) : (
        <div className={styles.insightRows}>
          <div className={styles.insightRow}>
            <span className={`${styles.insightLabel} ${styles.insightLabelYou}`}>● {m.comparison.you}</span>
            {insight.yourText
              ? <p className={styles.insightText}>{insight.yourText}</p>
              : <p className={styles.insightTextAbsent}>{m.comparison.absentYou}</p>
            }
          </div>
          <div className={styles.insightRow}>
            <span className={`${styles.insightLabel} ${styles.insightLabelThem}`}>● {otherLabel}</span>
            {insight.theirText
              ? <p className={styles.insightText}>{insight.theirText}</p>
              : <p className={styles.insightTextAbsent}>{m.comparison.absentThem(otherLabel)}</p>
            }
          </div>
        </div>
      )}
    </article>
  )
}

export function ComparisonPage({ comparison, onBack, languageSwitcher }: Readonly<Props>) {
  const m = useMessages()
  const allInsights = comparison.groups.flatMap(g => g.insights)
  const alignedInsights = allInsights.filter(i => i.classification === 'similarity')
  const differsInsights = allInsights.filter(i => i.classification === 'difference')

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        {onBack ? (
          <button className={styles.backLink} onClick={onBack}>{m.comparison.back}</button>
        ) : (
          <span />
        )}
        <span className={styles.headerEnd}>
          {languageSwitcher}
          <span className={styles.brand}>common ground</span>
        </span>
      </header>

      <main className={styles.content}>
        <p className={styles.eyebrow}>{m.comparison.eyebrow}</p>
        <h1 className={styles.title}>{m.comparison.title}</h1>

        <div className={styles.legend}>
          <span className={styles.legendYou}>● {m.comparison.you}</span>
          <span className={styles.legendDash}>——</span>
          <span className={styles.legendThem}>● {comparison.otherLabel}</span>
        </div>

        <div className={styles.summary}>{m.comparison.summary(alignedInsights.length, allInsights.length)}</div>

        {differsInsights.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionHeader}>{m.comparison.differHeading}</h2>
            <div className={styles.insightList}>
              {differsInsights.map(i => (
                <InsightCard key={i.dimensionId} insight={i} otherLabel={comparison.otherLabel} />
              ))}
            </div>
          </section>
        )}

        {alignedInsights.length > 0 && (
          <section className={styles.section}>
            <h2 className={styles.sectionHeader}>{m.comparison.alignedHeading}</h2>
            <div className={styles.insightList}>
              {alignedInsights.map(i => (
                <InsightCard key={i.dimensionId} insight={i} otherLabel={comparison.otherLabel} />
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
