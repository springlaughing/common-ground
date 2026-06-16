import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { renderWithLocale } from '../support/renderWithLocale'
import { ConsentStep } from '../../src/components/ConsentStep/ConsentStep'
import { ProgressIndicator } from '../../src/components/ProgressIndicator/ProgressIndicator'
import { QuestionStep } from '../../src/components/QuestionStep/QuestionStep'
import { CompletionStep } from '../../src/components/CompletionStep/CompletionStep'
import { ReflectionPage } from '../../src/pages/ReflectionPage/ReflectionPage'
import type { Question, ReflectionDto } from '../../src/types/api'

// T018 (US1): with locale = de, the UI chrome renders in German. Localized content
// (questions, group titles, insights) comes from the API and is passed in as props.
describe('US1 — UI chrome renders in German when locale = de', () => {
  it('ConsentStep shows the German heading, the approved privacy copy, and the begin button', () => {
    renderWithLocale(<ConsentStep onAcknowledge={vi.fn()} onBack={vi.fn()} />, 'de')

    expect(screen.getByRole('heading', { name: 'Bevor wir beginnen' })).toBeInTheDocument()
    expect(screen.getByText(/Deine einzelnen Antworten bleiben privat\./)).toBeInTheDocument()
    expect(screen.getByText('Ich bin einverstanden und möchte beginnen.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Beginnen/ })).toBeInTheDocument()
    // The language switcher is present (so a user can switch at the start of the flow).
    expect(screen.getByRole('group', { name: /Sprache/ })).toBeInTheDocument()
  })

  it('ProgressIndicator shows German counters (visible text + aria labels)', () => {
    renderWithLocale(
      <ProgressIndicator current={3} total={46} sectionCurrent={2} sectionTotal={10} />,
      'de',
    )

    expect(screen.getByText('Frage 3 / 46')).toBeInTheDocument()
    expect(screen.getByLabelText('Frage 3 von 46')).toBeInTheDocument()
    expect(screen.getByText('Abschnitt 2 von 10')).toBeInTheDocument()
  })

  it('QuestionStep shows German navigation labels', () => {
    const question: Question = {
      id: 'q1', text: 'Frage?', sectionIndex: 1, orderIndex: 1,
      answerOptions: [
        { id: 'a', text: 'A', orderIndex: 1 },
        { id: 'b', text: 'B', orderIndex: 2 },
      ],
    }
    renderWithLocale(
      <QuestionStep
        question={question}
        questionNumber={1}
        totalQuestions={46}
        sectionNumber={1}
        totalSections={10}
        primaryAnswerId="a"
        secondaryAnswerId={null}
        onSelectPrimary={vi.fn()}
        onSelectSecondary={vi.fn()}
        onNext={vi.fn()}
        onBack={vi.fn()}
        isFirst={false}
        isLast={false}
      />,
      'de',
    )

    expect(screen.getByRole('button', { name: /Weiter/ })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Zurück/ })).toBeInTheDocument()
  })

  it('CompletionStep shows the German heading', () => {
    renderWithLocale(
      <CompletionStep privateResultLink="/me#T" accessCode="A-B-C" onViewReflection={vi.fn()} />,
      'de',
    )

    expect(screen.getByText('Deine Reflexion ist fertig.')).toBeInTheDocument()
  })

  it('ReflectionPage shows German chrome around the API-provided content', () => {
    const reflection: ReflectionDto = {
      groups: [
        {
          id: 'g1',
          title: 'Wie du planst', // from the API (already localized server-side)
          insights: [{ dimensionId: 'd1', title: 'Titel', text: 'Inhalt', strength: 4 }],
        },
      ],
    }
    renderWithLocale(<ReflectionPage reflection={reflection} />, 'de')

    expect(screen.getByText('Dein Ergebnis')).toBeInTheDocument()
    expect(screen.getByText('Wie du arbeitest')).toBeInTheDocument()
    expect(screen.getByText('Wie du planst')).toBeInTheDocument()
  })
})
