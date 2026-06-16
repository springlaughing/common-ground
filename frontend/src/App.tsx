import { type CSSProperties, useEffect, useMemo, useState } from 'react'
import { WelcomeStep } from './components/WelcomeStep/WelcomeStep'
import { ConsentStep } from './components/ConsentStep/ConsentStep'
import { QuestionStep } from './components/QuestionStep/QuestionStep'
import { CompletionStep } from './components/CompletionStep/CompletionStep'
import { ReflectionPage } from './pages/ReflectionPage/ReflectionPage'
import { ComparisonPage } from './pages/ComparisonPage/ComparisonPage'
import type {
  AnswerSubmission,
  ComparisonDto,
  Question,
  ReflectionDto,
  SubmitResponseRequest,
  SubmitResponseResult,
} from './types/api'
import { ApiError, fetchCurrentQuestionnaire, submitResponses } from './services/questionnaireApi'
import { LanguageProvider, useLanguage } from './i18n/LanguageContext'
import { useMessages } from './i18n/useMessages'

// Comparison is a separate feature (not part of US1) and is not yet wired to the
// API, so it still renders from demo data until the comparison endpoints exist.
const DEMO_COMPARISON: ComparisonDto = {
  theirName: 'Jordan',
  summary: "You're closely aligned on planning rhythms and what motivates the work — both focused on craft and predictable cycles. The main gaps are communication format and how much mid-cycle flexibility each of you can absorb.",
  groups: [
    {
      id: 'work_context_expectations_and_alignment',
      title: 'Work context, expectations, and alignment',
      insights: [
        {
          // You scored above threshold; Jordan did not
          dimensionId: 'clarity_via_written_context',
          title: 'Written records over memory',
          yourStrength: 4,
          theirStrength: null,
          yourText: "You trust written records more than memory or conversation. Decisions that only exist in someone's head — or were said once in a meeting — are hard for you to rely on.",
        },
        {
          // Jordan scored above threshold; you did not
          dimensionId: 'verbal_alignment_preference',
          title: 'Live conversation over written exchange',
          yourStrength: null,
          theirStrength: 4,
          theirText: "Live conversation works better for them than written exchange. They can test their understanding, catch what's unsaid, and ask the follow-up that unlocks the real point — in a way a message thread rarely can.",
        },
      ],
    },
    {
      id: 'how_you_plan_and_handle_change',
      title: 'How you plan and handle change',
      insights: [
        {
          // Both scored — close, aligned
          dimensionId: 'iteration_preference',
          title: 'Fixed, predictable planning cycles',
          yourStrength: 5,
          theirStrength: 4,
          yourText: "Fixed cycles work well for you — a predictable cadence with clear moments to plan, deliver, and reflect. The rhythm itself helps you work at your best.",
          theirText: "Fixed cycles work well for them — a predictable cadence with clear moments to plan, deliver, and reflect. The rhythm itself helps them work at their best.",
        },
        {
          // Both scored — notable gap
          dimensionId: 'planning_boundary_protection',
          title: 'Protecting the plan from mid-cycle interruption',
          yourStrength: 4,
          theirStrength: 2,
          yourText: "You need the plan to be protected from constant interruption. Collecting changes and handling them at the next planning point — rather than mid-flow — is how you stay productive and make commitments that mean something.",
          theirText: "They can absorb changes to scope or direction without much friction. When something important comes up, they'd rather respond to it than protect the original plan for its own sake.",
        },
      ],
    },
    {
      id: 'how_you_handle_feedback',
      title: 'How you handle feedback',
      insights: [
        {
          // Both scored — fairly close
          dimensionId: 'feedback_content_primacy_receiving',
          title: 'Focusing on substance, not how feedback is delivered',
          yourStrength: 4,
          theirStrength: 3,
          yourText: "When you receive feedback, you focus on the substance — even if the delivery was clumsy or harsh. You don't let style get in the way of hearing what might be a valid point.",
          theirText: "When they receive feedback, they focus on the substance — even if the delivery was clumsy or harsh. They don't let style get in the way of hearing what might be a valid point.",
        },
      ],
    },
    {
      id: 'what_gives_you_energy_and_meaning',
      title: 'What gives you energy and meaning',
      insights: [
        {
          // Both scored — perfectly aligned
          dimensionId: 'craft_intrinsic_motivation',
          title: 'Driven by the work itself',
          yourStrength: 5,
          theirStrength: 5,
          yourText: "The work itself is what drives you. Building something well, solving a hard problem, getting the details right — that's satisfying in its own right, independent of whether anyone notices or what it's ultimately for.",
          theirText: "The work itself is what drives them. Building something well, solving a hard problem, getting the details right — that's satisfying in its own right, independent of whether anyone notices or what it's ultimately for.",
        },
        {
          // Both scored — notable gap
          dimensionId: 'focus_protection',
          title: 'Protecting uninterrupted focus',
          yourStrength: 3,
          theirStrength: 5,
          yourText: "Uninterrupted focus time matters to you. Interruptions, unnecessary meetings, and constant context switching have a real cost — and you notice when that cost isn't justified by what they produce.",
          theirText: "Uninterrupted focus time matters to them. Interruptions, unnecessary meetings, and constant context switching have a real cost — and they notice when that cost isn't justified by what they produce.",
        },
      ],
    },
  ],
}

// DEV-only sample reflection, used by the ?preview=reflection design harness below
// so we can fine-tune the page without the backend or walking the questionnaire.
// Strengths span 1–5 to exercise every dot state. Not referenced in production.
const DEMO_REFLECTION: ReflectionDto = {
  groups: [
    {
      id: 'work_context_expectations_and_alignment',
      title: 'Work context, expectations, and alignment',
      insights: [
        {
          dimensionId: 'clarity_via_written_context',
          title: 'Written records over memory',
          strength: 4,
          text: "You trust written records more than memory or conversation. Decisions that only exist in someone's head — or were said once in a meeting — are hard for you to rely on.",
        },
        {
          dimensionId: 'examples_over_description',
          title: 'Real examples over descriptions',
          strength: 3,
          text: "Real examples tell you more than descriptions do. You'd rather look at past code, a ticket, or an existing standard than read an explanation of how things work.",
        },
        {
          dimensionId: 'comfort_with_ambiguity',
          title: 'Starting before everything is settled',
          strength: 2,
          text: "You're comfortable starting before everything is settled. Not having all the answers yet doesn't stop you from moving — you'd rather begin and refine than wait for full certainty.",
        },
      ],
    },
    {
      id: 'how_you_plan_and_handle_change',
      title: 'How you plan and handle change',
      insights: [
        {
          dimensionId: 'iteration_preference',
          title: 'Fixed, predictable planning cycles',
          strength: 5,
          text: 'Fixed cycles work well for you — a predictable cadence with clear moments to plan, deliver, and reflect. The rhythm itself helps you work at your best.',
        },
        {
          dimensionId: 'planning_boundary_protection',
          title: 'Protecting the plan from interruption',
          strength: 4,
          text: 'You need the plan protected from constant interruption. Collecting changes and handling them at the next planning point — rather than mid-flow — is how you stay productive.',
        },
      ],
    },
    {
      id: 'what_gives_you_energy_and_meaning',
      title: 'What gives you energy and meaning',
      insights: [
        {
          dimensionId: 'craft_intrinsic_motivation',
          title: 'Driven by the work itself',
          strength: 5,
          text: "The work itself is what drives you. Building something well, solving a hard problem, getting the details right — that's satisfying in its own right.",
        },
        {
          dimensionId: 'focus_protection',
          title: 'Protecting uninterrupted focus',
          strength: 1,
          text: "Uninterrupted focus time matters to you. Interruptions and constant context switching have a real cost — and you notice when that cost isn't justified.",
        },
      ],
    },
  ],
}

type AnswerState = { primary: string | null; secondary: string | null }
type Stage = 'welcome' | 'consent' | 'questionnaire' | 'completion' | 'reflection' | 'comparison'

const centered: CSSProperties = {
  display: 'flex', alignItems: 'center', justifyContent: 'center',
  minHeight: '100vh', padding: '2rem', textAlign: 'center', flexDirection: 'column', gap: '1rem',
}

function AppInner() {
  const { locale } = useLanguage()
  const m = useMessages()
  const [stage, setStage] = useState<Stage>('welcome')
  const [idx, setIdx] = useState(0)
  const [answers, setAnswers] = useState<Record<string, AnswerState>>({})

  const [questions, setQuestions] = useState<Question[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)
  const [result, setResult] = useState<SubmitResponseResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  // Load the active questionnaire in the active locale; re-fetch when the locale changes
  // so switching language updates the questions. In-progress answers persist — they're
  // keyed by stable option IDs, which are locale-invariant.
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

  // "Section Y of N" — derived from the loaded questionnaire, not hardcoded.
  const totalSections = useMemo(
    () => questions.reduce((max, q) => Math.max(max, q.sectionIndex), 0) || 1,
    [questions],
  )

  async function handleSubmit() {
    if (submitting) return
    setSubmitting(true)
    setSubmitError(null)
    try {
      // The UI gates "Next/Submit" on a primary selection, so every question has one.
      const request: SubmitResponseRequest = {
        answers: questions.map((q): AnswerSubmission => {
          const a = answers[q.id] ?? { primary: null, secondary: null }
          const sub: AnswerSubmission = {
            questionId: q.id,
            primaryAnswerOptionId: a.primary as string,
          }
          if (a.secondary) sub.secondaryAnswerOptionId = a.secondary
          return sub
        }),
      }
      const res = await submitResponses(request, locale)
      setResult(res)
      setStage('completion')
    } catch (e: unknown) {
      setSubmitError(e instanceof ApiError ? e.message : m.status.submitFailed)
    } finally {
      setSubmitting(false)
    }
  }

  // DEV-only design preview: render one page in isolation from demo data so we can
  // fine-tune styling without the backend or walking the questionnaire. Try:
  //   /?preview=reflection   ·   /?preview=comparison
  // import.meta.env.DEV is statically false in production, so this whole block (and
  // DEMO_REFLECTION) is stripped from the prod bundle. It's excluded from coverage
  // (v8 ignore) rather than tested: it's dev scaffolding that never ships, so a test
  // for it would only game the coverage gate without guarding any production behaviour.
  /* v8 ignore start */
  if (import.meta.env.DEV) {
    const preview = new URLSearchParams(globalThis.location.search).get('preview')
    if (preview === 'reflection') {
      return (
        <ReflectionPage
          reflection={DEMO_REFLECTION}
          onCompare={() => { globalThis.location.search = '?preview=comparison' }}
        />
      )
    }
    if (preview === 'comparison') {
      return (
        <ComparisonPage
          comparison={DEMO_COMPARISON}
          onBack={() => { globalThis.location.search = '?preview=reflection' }}
        />
      )
    }
  }
  /* v8 ignore stop */

  if (stage === 'welcome') {
    return <WelcomeStep onStart={() => setStage('consent')} />
  }

  if (stage === 'consent') {
    return (
      <ConsentStep
        onBack={() => setStage('welcome')}
        onAcknowledge={() => setStage('questionnaire')}
      />
    )
  }

  if (stage === 'completion') {
    return (
      <CompletionStep
        privateResultLink={result?.privateResultLink ?? ''}
        accessCode={result?.accessCode ?? ''}
        onViewReflection={() => setStage('reflection')}
        onBack={() => {
          setIdx(questions.length - 1)
          setStage('questionnaire')
        }}
      />
    )
  }

  if (stage === 'reflection') {
    return (
      <ReflectionPage
        reflection={result?.reflection ?? { groups: [] }}
        onCompare={() => setStage('comparison')}
      />
    )
  }

  if (stage === 'comparison') {
    return (
      <ComparisonPage
        comparison={DEMO_COMPARISON}
        onBack={() => setStage('reflection')}
      />
    )
  }

  // stage === 'questionnaire'
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
    setAnswers(prev => ({
      ...prev,
      [question.id]: { primary: optionId, secondary: null },
    }))
  }

  function handleSelectSecondary(optionId: string | null) {
    setAnswers(prev => ({
      ...prev,
      [question.id]: {
        primary: prev[question.id]?.primary ?? null,
        secondary: optionId,
      },
    }))
  }

  return (
    <>
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
          if (isLastQuestion) void handleSubmit()
          else setIdx(i => i + 1)
        }}
        onBack={() => {
          if (idx === 0) setStage('consent')
          else setIdx(i => i - 1)
        }}
        isFirst={idx === 0}
        isLast={isLastQuestion}
      />
      {(submitting || submitError) && (
        <div
          role={submitError ? 'alert' : 'status'}
          style={{
            position: 'fixed', left: '50%', bottom: '1rem', transform: 'translateX(-50%)',
            background: submitError ? '#b00020' : '#333', color: '#fff',
            padding: '0.6rem 1rem', borderRadius: 8, maxWidth: '90%',
          }}
        >
          {submitError ?? m.status.submitting}
        </div>
      )}
    </>
  )
}

export default function App() {
  return (
    <LanguageProvider>
      <AppInner />
    </LanguageProvider>
  )
}
