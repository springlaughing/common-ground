import type { Question } from '../../types/api'
import { ProgressIndicator } from '../ProgressIndicator/ProgressIndicator'
import styles from './QuestionStep.module.css'

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
}: Props) {
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

  const isLastQuestion = questionNumber === totalQuestions

  return (
    <div className={styles.step}>
      {/* Top progress bar — full width */}
      <div
        className={styles.progressBar}
        role="progressbar"
        aria-valuenow={Math.round(progressPct)}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`${Math.round(progressPct)}% complete`}
      >
        <div className={styles.progressFill} style={{ width: `${progressPct}%` }} />
      </div>

      {/* Header: back link left, brand right */}
      <header className={styles.header}>
        {!isFirst ? (
          <button className={styles.backLink} onClick={onBack}>
            ← Previous
          </button>
        ) : (
          <span />
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

        <div className={styles.cards} role="group" aria-label="Answer options">
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
                aria-label={`${option.text}${state === 'primary' ? ' — your primary choice' : state === 'secondary' ? ' — your secondary choice' : ''}`}
              >
                <span className={styles.dot} aria-hidden="true">
                  {state === 'primary' && '1'}
                  {state === 'secondary' && '2'}
                </span>
                <span className={styles.cardText}>{option.text}</span>
              </button>
            )
          })}
        </div>

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
          {isLastQuestion ? 'Submit' : 'Next'} →
        </button>
      </footer>
    </div>
  )
}
