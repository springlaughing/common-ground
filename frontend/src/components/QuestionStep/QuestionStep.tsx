import type { Question } from '../../types/api'
import { ProgressIndicator } from '../ProgressIndicator/ProgressIndicator'
import styles from './QuestionStep.module.css'

function choiceSuffix(state: 'primary' | 'secondary' | 'none'): string {
  if (state === 'primary') return ' — your primary choice'
  if (state === 'secondary') return ' — your secondary choice'
  return ''
}

interface Props {
  question: Question
  questionNumber: number
  totalQuestions: number
  sectionNumber: number
  totalSections: number
  primaryAnswerId: string | null
  secondaryAnswerId: string | null
  onSelectPrimary: (optionId: string | null) => void
  onSelectSecondary: (optionId: string | null) => void
  onNext: () => void
  onBack: () => void
  isFirst: boolean
  /** When true, the advance button reads "Submit" instead of "Next". */
  isLast: boolean
}

export function QuestionStep({
  question,
  questionNumber,
  totalQuestions,
  sectionNumber,
  totalSections,
  primaryAnswerId,
  secondaryAnswerId,
  onSelectPrimary,
  onSelectSecondary,
  onNext,
  onBack,
  isFirst,
  isLast,
}: Readonly<Props>) {
  const progressPct = ((questionNumber - 1) / totalQuestions) * 100

  function handleCardClick(optionId: string) {
    if (optionId === primaryAnswerId) {
      onSelectPrimary(null)
      onSelectSecondary(null)
      return
    }
    if (optionId === secondaryAnswerId) {
      onSelectSecondary(null)
      return
    }
    if (primaryAnswerId === null) {
      onSelectPrimary(optionId)
      return
    }
    onSelectSecondary(optionId)
  }

  function getState(optionId: string): 'primary' | 'secondary' | 'none' {
    if (optionId === primaryAnswerId) return 'primary'
    if (optionId === secondaryAnswerId) return 'secondary'
    return 'none'
  }

  return (
    <div className={styles.step}>
      {/* Top progress bar — full width */}
      <div className={styles.progressBar} aria-hidden="true">
        <div className={styles.progressFill} style={{ width: `${progressPct}%` }} />
      </div>

      {/* Header: back link left, brand right */}
      <header className={styles.header}>
        {isFirst ? (
          <span />
        ) : (
          <button className={styles.backLink} onClick={onBack}>
            ← Previous
          </button>
        )}
        <span className={styles.brand}>common ground</span>
      </header>

      {/* Main scrollable content */}
      <main className={styles.content}>
        <ProgressIndicator
          current={questionNumber}
          total={totalQuestions}
          sectionCurrent={sectionNumber}
          sectionTotal={totalSections}
        />

        <h1 className={styles.question}>{question.text}</h1>

        <fieldset className={styles.cards} aria-label="Answer options">
          {question.answerOptions.map(option => {
            const state = getState(option.id)
            return (
              <button
                key={option.id}
                className={[
                  styles.card,
                  state === 'primary' ? styles.primary : '',
                  state === 'secondary' ? styles.secondary : '',
                ].filter(Boolean).join(' ')}
                onClick={() => handleCardClick(option.id)}
                aria-pressed={state !== 'none'}
                aria-label={`${option.text}${choiceSuffix(state)}`}
              >
                <span className={styles.dot} aria-hidden="true">
                  {state === 'primary' && '1'}
                  {state === 'secondary' && '2'}
                </span>
                <span className={styles.cardText}>{option.text}</span>
              </button>
            )
          })}
        </fieldset>

        {primaryAnswerId !== null && (
          <p className={styles.hint}>
            {secondaryAnswerId === null && 'You can optionally pick a second preference. '}
            Tap a selected answer again to undo it.
          </p>
        )}
      </main>

      {/* Decorative brand initials — behind content */}
      <div className={styles.deco} aria-hidden="true">
        <span className={styles.c}>c</span>
        <span className={styles.g}>g</span>
      </div>

      {/* Footer: next / submit */}
      <footer className={styles.footer}>
        <button
          className={styles.nextBtn}
          onClick={onNext}
          disabled={primaryAnswerId === null}
        >
          {isLast ? 'Submit' : 'Next'} →
        </button>
      </footer>
    </div>
  )
}
