import { useState } from 'react'
import { WelcomeStep } from './components/WelcomeStep/WelcomeStep'
import { ConsentStep } from './components/ConsentStep/ConsentStep'
import { QuestionStep } from './components/QuestionStep/QuestionStep'
import type { Question } from './types/api'

const DEMO_QUESTIONS: Question[] = [
  {
    id: 'q1',
    text: 'When working on a challenging project, what is your default approach?',
    sectionIndex: 1,
    orderIndex: 1,
    answerOptions: [
      { id: 'a1', text: 'I dive in and figure things out as I go — iteration beats planning.', orderIndex: 1 },
      { id: 'a2', text: 'I map out the full problem before writing a single line of code.', orderIndex: 2 },
      { id: 'a3', text: 'I look for what others have already built that I can learn from.', orderIndex: 3 },
      { id: 'a4', text: 'I talk it through with someone else first — two perspectives always help.', orderIndex: 4 },
    ],
  },
  {
    id: 'q2',
    text: 'How do you prefer to communicate with your team day-to-day?',
    sectionIndex: 1,
    orderIndex: 2,
    answerOptions: [
      { id: 'b1', text: 'Quick async messages — I like switching contexts on my own schedule.', orderIndex: 1 },
      { id: 'b2', text: 'Structured meetings with a clear agenda and defined outcomes.', orderIndex: 2 },
      { id: 'b3', text: 'Informal chats whenever something comes up — keeps the energy high.', orderIndex: 3 },
      { id: 'b4', text: 'Written documentation first, then sync only when truly needed.', orderIndex: 4 },
    ],
  },
  {
    id: 'q3',
    text: 'When you receive critical feedback on your work, what is your first instinct?',
    sectionIndex: 2,
    orderIndex: 3,
    answerOptions: [
      { id: 'c1', text: 'Ask clarifying questions to fully understand the concern before reacting.', orderIndex: 1 },
      { id: 'c2', text: 'Take time alone to process it before responding.', orderIndex: 2 },
      { id: 'c3', text: 'Push back if the feedback is missing important context.', orderIndex: 3 },
      { id: 'c4', text: 'Immediately start thinking about how to act on it.', orderIndex: 4 },
    ],
  },
]

type AnswerState = { primary: string | null; secondary: string | null }
type Stage = 'welcome' | 'consent' | 'questionnaire'

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
      onNext={() => setIdx(i => Math.min(i + 1, DEMO_QUESTIONS.length - 1))}
      onBack={() => {
        if (idx === 0) setStage('consent')
        else setIdx(i => i - 1)
      }}
      isFirst={false}
    />
  )
}
