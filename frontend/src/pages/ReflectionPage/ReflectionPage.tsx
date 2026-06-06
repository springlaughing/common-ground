import { InsightCard } from '../../components/InsightCard/InsightCard'
import type { ReflectionDto } from '../../types/api'
import styles from './ReflectionPage.module.css'

interface Props {
  reflection: ReflectionDto
  onCompare?: () => void
}

export function ReflectionPage({ reflection, onCompare }: Readonly<Props>) {
  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <span />
        <span className={styles.brand}>common ground</span>
      </header>

      <main className={styles.content}>
        <p className={styles.eyebrow}>Your reflection</p>
        <h1 className={styles.title}>How you work</h1>
        <p className={styles.intro}>
          Based on your answers. These are observations about how you tend to work — not
          scores, not a rating, and nothing here is better or worse than its opposite.
        </p>

        {reflection.groups.map(group => (
          <section key={group.id} className={styles.group}>
            <h2 className={styles.groupTitle}>{group.title}</h2>
            <div className={styles.insights}>
              {group.insights.map(insight => (
                <InsightCard
                  key={insight.dimensionId}
                  title={insight.title}
                  text={insight.text}
                  strength={insight.strength}
                />
              ))}
            </div>
          </section>
        ))}

        <p className={styles.footnote}>
          Only the patterns your answers signal clearly are shown here — that's why some
          themes may not appear.
        </p>

        {onCompare && (
          <div className={styles.compareBlock}>
            <button className={styles.compareCta} onClick={onCompare}>
              Compare with someone →
            </button>
          </div>
        )}
      </main>

      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>
    </div>
  )
}
