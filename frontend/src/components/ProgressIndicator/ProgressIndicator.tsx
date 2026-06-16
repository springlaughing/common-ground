import { useMessages } from '../../i18n/useMessages'
import styles from './ProgressIndicator.module.css'

interface Props {
  current: number
  total: number
  sectionCurrent: number
  sectionTotal: number
}

export function ProgressIndicator({ current, total, sectionCurrent, sectionTotal }: Readonly<Props>) {
  const m = useMessages()

  return (
    <div className={styles.root}>
      <span
        className={styles.counter}
        aria-label={m.progress.questionAria(current, total)}
      >
        {m.progress.questionCounter(current, total)}
      </span>
      <span
        className={styles.section}
        aria-label={m.progress.sectionLabel(sectionCurrent, sectionTotal)}
      >
        {m.progress.sectionLabel(sectionCurrent, sectionTotal)}
      </span>
    </div>
  )
}
