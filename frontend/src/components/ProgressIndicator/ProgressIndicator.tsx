import styles from './ProgressIndicator.module.css'

interface Props {
  current: number
  total: number
  sectionCurrent: number
  sectionTotal: number
}

export function ProgressIndicator({ current, total, sectionCurrent, sectionTotal }: Readonly<Props>) {
  return (
    <div className={styles.root}>
      <span
        className={styles.counter}
        aria-label={`Question ${current} of ${total}`}
      >
        Question {current} / {total}
      </span>
      <span
        className={styles.section}
        aria-label={`Section ${sectionCurrent} of ${sectionTotal}`}
      >
        Section {sectionCurrent} of {sectionTotal}
      </span>
    </div>
  )
}
