import { useState } from 'react'
import { WelcomeStep } from './components/WelcomeStep/WelcomeStep'
import { ConsentStep } from './components/ConsentStep/ConsentStep'
import { QuestionStep } from './components/QuestionStep/QuestionStep'
import { CompletionStep } from './components/CompletionStep/CompletionStep'
import { ReflectionPage } from './pages/ReflectionPage/ReflectionPage'
import { ComparisonPage } from './pages/ComparisonPage/ComparisonPage'
import type { Question, ReflectionDto, ComparisonDto } from './types/api'

const DEMO_QUESTIONS: Question[] = [
  {
    id: 'q-s1-1',
    text: 'When starting work with someone new, what helps you feel ready to collaborate?',
    sectionIndex: 1,
    orderIndex: 1,
    answerOptions: [
      { id: 'q-s1-1-a', text: 'Written goals, context, and expectations — I need to understand what we are working toward before I can contribute. A document I can return to is more reliable than a conversation I have to remember.', orderIndex: 1 },
      { id: 'q-s1-1-b', text: 'A live conversation to ask questions and calibrate — written context only tells me what someone decided to write down. A real exchange lets me test my understanding and surface what I did not know I needed to ask.', orderIndex: 2 },
      { id: 'q-s1-1-c', text: 'Seeing examples or trying a small piece of real work — I learn more about how a collaboration works by doing something together than by discussing it first.', orderIndex: 3 },
      { id: 'q-s1-1-d', text: 'Clear ownership and decision boundaries outlined somewhere, and discussion about them when needed — I can move quickly once I know who is accountable for what and where my judgment applies.', orderIndex: 4 },
    ],
  },
  {
    id: 'q-s8-4',
    text: 'During a high-pressure or urgent situation, someone tells you that your tone, word choice, or communication style felt too intense, too blunt, or hard to receive. What is your instinct?',
    sectionIndex: 8,
    orderIndex: 4,
    answerOptions: [
      { id: 'q-s8-4-a', text: 'I take it seriously and adjust — if my delivery made it harder for someone to think clearly or act well, that is my problem to fix regardless of the pressure we were both under.', orderIndex: 1 },
      { id: 'q-s8-4-b', text: 'I want to understand what specifically landed badly before I change anything — even under pressure, adjusting without understanding what went wrong usually does not help either of us.', orderIndex: 2 },
      { id: 'q-s8-4-c', text: 'I acknowledge the reaction but do not automatically see it as something to fix — sometimes intensity reflects the situation honestly, and I think that can be said without apologizing for it.', orderIndex: 3 },
      { id: 'q-s8-4-d', text: 'I notice whether it is a pattern or a one-off — pressure situations are not normal conditions, and a single reaction does not always mean I need to change how I communicate generally.', orderIndex: 4 },
    ],
  },
]

// Mock submission result — real snippet text from reflection-groups.json.
// Only 4 of the 10 groups appear, demonstrating that groups below the display
// threshold are omitted (FR-020).
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
          dimensionId: 'upfront_clarity_need',
          title: 'Clarity before commitment',
          strength: 3,
          text: 'You need goals, expectations, and the quality bar to be clear before you commit. Starting without that picture makes it hard to move with confidence.',
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
          title: 'Protecting the plan from mid-cycle interruption',
          strength: 4,
          text: 'You need the plan to be protected from constant interruption. Collecting changes and handling them at the next planning point — rather than mid-flow — is how you stay productive and make commitments that mean something.',
        },
      ],
    },
    {
      id: 'how_you_handle_feedback',
      title: 'How you handle feedback',
      insights: [
        {
          dimensionId: 'feedback_content_primacy_receiving',
          title: 'Substance in feedback (receiving)',
          strength: 4,
          text: "When you receive feedback, you focus on the substance — even if the delivery was clumsy or harsh. You don't let style get in the way of hearing what might be a valid point.",
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
          text: "The work itself is what drives you. Building something well, solving a hard problem, getting the details right — that's satisfying in its own right, independent of whether anyone notices or what it's ultimately for.",
        },
        {
          dimensionId: 'focus_protection',
          title: 'Protecting uninterrupted focus',
          strength: 3,
          text: "Uninterrupted focus time matters to you. Interruptions, unnecessary meetings, and constant context switching have a real cost — and you notice when that cost isn't justified by what they produce.",
        },
      ],
    },
  ],
}

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

const DEMO_PRIVATE_LINK = '/me#a7F3kQ9xL2mNpR8vT4wYbZ'
const DEMO_ACCESS_CODE = 'K7Q9-MP2D-W4T8'

type AnswerState = { primary: string | null; secondary: string | null }
type Stage = 'welcome' | 'consent' | 'questionnaire' | 'completion' | 'reflection' | 'comparison'

export default function App() {
  const [stage, setStage] = useState<Stage>('welcome')
  const [idx, setIdx] = useState(0)
  const [answers, setAnswers] = useState<Record<string, AnswerState>>({})

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
        privateResultLink={DEMO_PRIVATE_LINK}
        accessCode={DEMO_ACCESS_CODE}
        onViewReflection={() => setStage('reflection')}
        onBack={() => {
          setIdx(DEMO_QUESTIONS.length - 1)
          setStage('questionnaire')
        }}
      />
    )
  }

  if (stage === 'reflection') {
    return (
      <ReflectionPage
        reflection={DEMO_REFLECTION}
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

  const isLastQuestion = idx === DEMO_QUESTIONS.length - 1
  const question = DEMO_QUESTIONS[idx]
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
    <QuestionStep
      question={question}
      questionNumber={idx + 1}
      totalQuestions={46}
      sectionNumber={question.sectionIndex}
      totalSections={10}
      primaryAnswerId={current.primary}
      secondaryAnswerId={current.secondary}
      onSelectPrimary={handleSelectPrimary}
      onSelectSecondary={handleSelectSecondary}
      onNext={() => {
        if (isLastQuestion) setStage('completion')
        else setIdx(i => i + 1)
      }}
      onBack={() => {
        if (idx === 0) setStage('consent')
        else setIdx(i => i - 1)
      }}
      isFirst={idx === 0}
      isLast={isLastQuestion}
    />
  )
}
