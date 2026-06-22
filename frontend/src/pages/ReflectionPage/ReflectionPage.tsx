import type { ReactNode } from 'react'
import { InsightCard } from '../../components/InsightCard/InsightCard'
import { useMessages } from '../../i18n/useMessages'
import type { ReflectionDto } from '../../types/api'
import styles from './ReflectionPage.module.css'

interface Props {
  reflection: ReflectionDto
  onCompare?: () => void
  /** Optional header-left content (the saved /me reflection mounts the language switcher
   *  here so it's switchable at view time; the post-submit flow leaves it empty). */
  languageSwitcher?: ReactNode
  /** Optional content below the reflection — the /me hub mounts its comparison list here. */
  hubContent?: ReactNode
}

export function ReflectionPage({ reflection, onCompare, languageSwitcher, hubContent }: Readonly<Props>) {
  const m = useMessages()

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        {languageSwitcher ?? <span />}
        <span className={styles.brand}>common ground</span>
      </header>

      <main className={styles.content}>
        <p className={styles.eyebrow}>{m.reflection.eyebrow}</p>
        <h1 className={styles.title}>{m.reflection.title}</h1>
        <p className={styles.intro}>{m.reflection.intro}</p>

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

        <p className={styles.footnote}>{m.reflection.footnote}</p>

        {onCompare && (
          <div className={styles.compareBlock}>
            <button className={styles.compareCta} onClick={onCompare}>
              {m.reflection.compare}
            </button>
          </div>
        )}

        {hubContent && <div className={styles.hub}>{hubContent}</div>}
      </main>

      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>
    </div>
  )
}
