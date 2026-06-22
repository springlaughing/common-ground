import { useState } from 'react'
import { WelcomeStep } from './components/WelcomeStep/WelcomeStep'
import { ConsentStep } from './components/ConsentStep/ConsentStep'
import { QuestionnaireFlow } from './components/QuestionnaireFlow/QuestionnaireFlow'
import { CompletionStep } from './components/CompletionStep/CompletionStep'
import { ReflectionPage } from './pages/ReflectionPage/ReflectionPage'
import { InviteCreate } from './components/InviteCreate/InviteCreate'
import type {
  AnswerSubmission,
  ReflectionDto,
  SubmitResponseResult,
} from './types/api'
import { ApiError, startSession, submitResponses } from './services/questionnaireApi'
import { LanguageProvider, useLanguage } from './i18n/LanguageContext'
import { useMessages } from './i18n/useMessages'

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

type Stage = 'welcome' | 'consent' | 'questionnaire' | 'completion' | 'reflection'

function AppInner() {
  const { locale } = useLanguage()
  const m = useMessages()
  const [stage, setStage] = useState<Stage>('welcome')
  const [result, setResult] = useState<SubmitResponseResult | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  // Retained so "back" from completion can reopen the questionnaire with the answers intact.
  const [lastAnswers, setLastAnswers] = useState<AnswerSubmission[] | undefined>(undefined)
  const [comparing, setComparing] = useState(false)

  // "Compare" from a just-finished reflection: start a session from the freshly minted token so
  // the invite POST is authenticated, then open the invite panel (same as the /me hub).
  async function startComparing() {
    const token = result?.privateResultLink?.split('#')[1]
    if (token) {
      try {
        await startSession(token)
      } catch {
        // If the session can't start, InviteCreate will surface the failure on submit.
      }
    }
    setComparing(true)
  }

  async function handleComplete(answers: AnswerSubmission[]) {
    if (submitting) return
    setLastAnswers(answers)
    setSubmitting(true)
    setSubmitError(null)
    try {
      const res = await submitResponses({ answers }, locale)
      setResult(res)
      setStage('completion')
    } catch (e: unknown) {
      setSubmitError(e instanceof ApiError ? e.message : m.status.submitFailed)
    } finally {
      setSubmitting(false)
    }
  }

  // DEV-only design preview: render the reflection in isolation from demo data so we can
  // fine-tune styling without the backend or walking the questionnaire. Try /?preview=reflection.
  // import.meta.env.DEV is statically false in production, so this whole block (and
  // DEMO_REFLECTION) is stripped from the prod bundle. It's excluded from coverage
  // (v8 ignore) rather than tested: it's dev scaffolding that never ships, so a test
  // for it would only game the coverage gate without guarding any production behaviour.
  /* v8 ignore start */
  if (import.meta.env.DEV && new URLSearchParams(globalThis.location.search).get('preview') === 'reflection') {
    return <ReflectionPage reflection={DEMO_REFLECTION} />
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
        onBack={() => setStage('questionnaire')}
      />
    )
  }

  if (stage === 'reflection') {
    return (
      <>
        <ReflectionPage
          reflection={result?.reflection ?? { groups: [] }}
          onCompare={startComparing}
        />
        {comparing && <InviteCreate onClose={() => setComparing(false)} />}
      </>
    )
  }

  // stage === 'questionnaire'
  return (
    <QuestionnaireFlow
      onComplete={handleComplete}
      onExit={() => setStage('consent')}
      submitting={submitting}
      submitError={submitError}
      initialAnswers={lastAnswers}
    />
  )
}

export default function App() {
  return (
    <LanguageProvider>
      <AppInner />
    </LanguageProvider>
  )
}
