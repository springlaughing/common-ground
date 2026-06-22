import { type CSSProperties, type ReactNode, useEffect, useMemo, useState } from 'react'
import { QuestionStep } from '../QuestionStep/QuestionStep'
import type { AnswerSubmission, Question } from '../../types/api'
import { fetchCurrentQuestionnaire } from '../../services/questionnaireApi'
import { useLanguage } from '../../i18n/LanguageContext'
import { useMessages } from '../../i18n/useMessages'

type AnswerState = { primary: string | null; secondary: string | null }

interface Props {
  /** Called with the completed answers when the last question is submitted. The parent owns the
   *  actual POST (normal submit vs. invitee join), so it controls `submitting`/`submitError`. */
  onComplete: (answers: AnswerSubmission[]) => void
  /** Back from the first question (the parent decides where that goes, e.g. the consent step). */
  onExit: () => void
  /** True while the parent's submit/join is in flight — drives the footer button state. */
  submitting: boolean
  /** A submit/join failure message to surface as a toast (the parent sets it). */
  submitError?: string | null
  /** Resume from previously collected answers (e.g. "back" from the completion step) — seeds the
   *  selections and opens at the last question. Omit for a fresh start at the first question. */
  initialAnswers?: AnswerSubmission[]
}

function seedAnswers(submissions?: AnswerSubmission[]): Record<string, AnswerState> {
  if (!submissions) return {}
  return Object.fromEntries(submissions.map(s => [
    s.questionId,
    { primary: s.primaryAnswerOptionId, secondary: s.secondaryAnswerOptionId ?? null },
  ]))
}

const centered: CSSProperties = {
  display: 'flex', alignItems: 'center', justifyContent: 'center',
  minHeight: '100vh', padding: '2rem', textAlign: 'center', flexDirection: 'column', gap: '1rem',
}

/** Walks a person through the active questionnaire and hands the collected answers back via
 *  `onComplete`. Shared by the first-time flow (App) and the invitee join (InvitePage) so the
 *  question-walking, locale re-fetch, and answer preservation live in exactly one place. */
export function QuestionnaireFlow({ onComplete, onExit, submitting, submitError, initialAnswers }: Readonly<Props>) {
  const { locale } = useLanguage()
  const m = useMessages()
  const [idx, setIdx] = useState(0)
  const [answers, setAnswers] = useState<Record<string, AnswerState>>(() => seedAnswers(initialAnswers))
  const [questions, setQuestions] = useState<Question[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)
  const [resumed, setResumed] = useState(false)

  // When resuming from prior answers, open at the last question (where the submit lives) once the
  // questionnaire has loaded — matching the pre-extraction "back from completion" behaviour.
  const resuming = (initialAnswers?.length ?? 0) > 0
  useEffect(() => {
    if (resuming && !resumed && questions.length > 0) {
      setIdx(questions.length - 1)
      setResumed(true)
    }
  }, [resuming, resumed, questions.length])

  // Load the active questionnaire in the active locale; re-fetch when the locale changes so
  // switching language updates the questions. In-progress answers persist — they're keyed by
  // stable option IDs, which are locale-invariant.
  useEffect(() => {
    const controller = new AbortController()
    setLoadError(null)
    fetchCurrentQuestionnaire(controller.signal, locale)
      .then(data => setQuestions(data.questions))
      .catch((e: unknown) => {
        if (e instanceof DOMException && e.name === 'AbortError') return
        setLoadError(e instanceof Error ? e.message : m.status.loadFailed)
      })
    return () => controller.abort()
  }, [locale, m])

  // Each new question starts at the top. The layouts scroll the window (no inner overflow
  // container), so advancing otherwise lands mid-page.
  useEffect(() => {
    window.scrollTo(0, 0)
  }, [idx])

  // "Section Y of N" — derived from the loaded questionnaire, not hardcoded.
  const totalSections = useMemo(
    () => questions.reduce((max, q) => Math.max(max, q.sectionIndex), 0) || 1,
    [questions],
  )

  if (loadError) {
    return (
      <div style={centered}>
        <p>{loadError}</p>
        <button onClick={() => globalThis.location.reload()}>{m.status.retry}</button>
      </div>
    )
  }

  if (questions.length === 0) {
    return <div style={centered}><p>{m.status.loading}</p></div>
  }

  const isLastQuestion = idx === questions.length - 1
  const question = questions[idx]
  const current = answers[question.id] ?? { primary: null, secondary: null }

  function handleSelectPrimary(optionId: string | null) {
    setAnswers(prev => ({ ...prev, [question.id]: { primary: optionId, secondary: null } }))
  }

  function handleSelectSecondary(optionId: string | null) {
    setAnswers(prev => ({
      ...prev,
      [question.id]: { primary: prev[question.id]?.primary ?? null, secondary: optionId },
    }))
  }

  function complete() {
    // The UI gates "Next/Submit" on a primary selection, so every question has one.
    const built = questions.map((q): AnswerSubmission => {
      const a = answers[q.id] ?? { primary: null, secondary: null }
      const sub: AnswerSubmission = { questionId: q.id, primaryAnswerOptionId: a.primary as string }
      if (a.secondary) sub.secondaryAnswerOptionId = a.secondary
      return sub
    })
    onComplete(built)
  }

  // DEV-only: open with ?dev to reveal a button that auto-fills a valid answer for every question
  // and jumps to the last one — so submit/completion polish can be tested without walking all 46.
  // import.meta.env.DEV is statically false in production, so this is stripped from the prod bundle
  // (and never tested — it's dev scaffolding, hence v8 ignore).
  /* v8 ignore start */
  let devSkip: ReactNode = null
  if (import.meta.env.DEV && new URLSearchParams(globalThis.location.search).has('dev')) {
    devSkip = (
      <button
        type="button"
        onClick={() => {
          setAnswers(Object.fromEntries(
            questions.map(q => [q.id, { primary: q.answerOptions[0].id, secondary: null }]),
          ))
          setIdx(questions.length - 1)
        }}
        style={{
          position: 'fixed', top: 8, left: 8, zIndex: 9999, fontSize: 12,
          padding: '4px 8px', background: '#b00020', color: '#fff',
          border: 'none', borderRadius: 6, cursor: 'pointer',
        }}
      >
        dev: skip to last
      </button>
    )
  }
  /* v8 ignore stop */

  return (
    <>
      {devSkip}
      <QuestionStep
        question={question}
        questionNumber={idx + 1}
        totalQuestions={questions.length}
        sectionNumber={question.sectionIndex}
        totalSections={totalSections}
        primaryAnswerId={current.primary}
        secondaryAnswerId={current.secondary}
        onSelectPrimary={handleSelectPrimary}
        onSelectSecondary={handleSelectSecondary}
        onNext={() => {
          if (isLastQuestion) complete()
          else setIdx(i => i + 1)
        }}
        onBack={() => {
          if (idx === 0) onExit()
          else setIdx(i => i - 1)
        }}
        isFirst={idx === 0}
        isLast={isLastQuestion}
        submitting={submitting}
      />
      {/* The submitting state lives on the footer button (QuestionStep); only a failure surfaces
          here, as a toast that won't overlap the button on mobile. */}
      {submitError && (
        <div
          role="alert"
          style={{
            position: 'fixed', left: '50%', bottom: '1rem', transform: 'translateX(-50%)',
            background: '#b00020', color: '#fff',
            padding: '0.6rem 1rem', borderRadius: 8, maxWidth: '90%',
          }}
        >
          {submitError}
        </div>
      )}
    </>
  )
}
