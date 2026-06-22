import type { ComparisonListItem } from '../../types/api'
import { useMessages } from '../../i18n/useMessages'
import styles from './ComparisonList.module.css'

interface Props {
  comparisons: ComparisonListItem[]
  /** Open a ready comparison's report. */
  onOpen: (comparisonId: string) => void
}

/** US4 (T039) — the /me hub's list of the viewer's comparisons. Ready ones can be opened; pending
 *  ones (the other person hasn't joined yet) and unavailable ones show a neutral status instead. */
export function ComparisonList({ comparisons, onOpen }: Readonly<Props>) {
  const m = useMessages()

  if (comparisons.length === 0)
    return <p className={styles.empty}>{m.comparisonList.empty}</p>

  return (
    <section className={styles.root}>
      <h2 className={styles.heading}>{m.comparisonList.heading}</h2>
      <ul className={styles.list}>
        {comparisons.map(c => (
          <li key={c.comparisonId} className={styles.item}>
            {c.status === 'complete' ? (
              <>
                <span className={styles.label}>{c.otherLabel}</span>
                <button type="button" className={styles.open} onClick={() => onOpen(c.comparisonId)}>
                  {m.comparisonList.open}
                </button>
              </>
            ) : (
              <>
                <span className={styles.label}>{c.otherLabel}</span>
                <span className={styles.status}>
                  {c.status === 'unavailable' ? m.comparisonList.unavailable : m.comparisonList.pending}
                </span>
              </>
            )}
          </li>
        ))}
      </ul>
    </section>
  )
}
