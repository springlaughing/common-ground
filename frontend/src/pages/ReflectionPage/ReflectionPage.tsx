import type { ReflectionDto } from '../../types/api'
import styles from './ReflectionPage.module.css'

interface Props {
  reflection: ReflectionDto
}

function StrengthIndicator({ strength }: { strength: number }) {
  return (
    <span
      className={styles.strength}
      role="img"
      aria-label={`Strength ${strength} out of 5`}
    >
      {[1, 2, 3, 4, 5].map(i => (
        <span
          key={i}
          className={`${styles.pip} ${i <= strength ? styles.pipOn : ''}`}
          aria-hidden="true"
        />
      ))}
    </span>
  )
}

export function ReflectionPage({ reflection }: Props) {
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
                <article key={insight.dimensionId} className={styles.insight}>
                  <StrengthIndicator strength={insight.strength} />
                  <p className={styles.insightText}>{insight.text}</p>
                </article>
              ))}
            </div>
          </section>
        ))}

        <p className={styles.footnote}>
          Only the patterns your answers signal clearly are shown here — that's why some
          themes may not appear.
        </p>
      </main>

      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>
    </div>
  )
}
