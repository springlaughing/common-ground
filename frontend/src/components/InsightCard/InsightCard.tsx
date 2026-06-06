import styles from './InsightCard.module.css'

interface Props {
  title: string
  text: string
  /** 1–5; the number of filled dots in the strength indicator. */
  strength: number
}

const DOTS = [1, 2, 3, 4, 5]

/** A single reflection insight: a title, a 5-dot strength indicator, and the
 *  explanatory text. Reused by the personal reflection and (later) comparison views. */
export function InsightCard({ title, text, strength }: Readonly<Props>) {
  return (
    <article className={styles.insight}>
      <h3 className={styles.insightTitle}>{title}</h3>
      <div className={styles.dots} role="img" aria-label={`Strength ${strength} out of 5`}>
        {DOTS.map(i => {
          const filled = i <= strength
          return (
            <span
              key={i}
              data-filled={filled}
              className={`${styles.dot} ${filled ? styles.dotFilled : styles.dotEmpty}`}
            />
          )
        })}
      </div>
      <p className={styles.insightText}>{text}</p>
    </article>
  )
}
