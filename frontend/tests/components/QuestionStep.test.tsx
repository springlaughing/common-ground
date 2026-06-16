import type { ComponentProps } from 'react'
import { screen } from '@testing-library/react'
import { renderWithLocale } from '../support/renderWithLocale'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { QuestionStep } from '../../src/components/QuestionStep/QuestionStep'
import type { Question } from '../../src/types/api'

const question: Question = {
  id: 'q1',
  text: 'How do you prefer to start?',
  sectionIndex: 1,
  orderIndex: 1,
  answerOptions: [
    { id: 'a', text: 'Option A', orderIndex: 1 },
    { id: 'b', text: 'Option B', orderIndex: 2 },
    { id: 'c', text: 'Option C', orderIndex: 3 },
    { id: 'd', text: 'Option D', orderIndex: 4 },
  ],
}

function renderStep(overrides: Partial<ComponentProps<typeof QuestionStep>> = {}) {
  const props = {
    question,
    questionNumber: 1,
    totalQuestions: 46,
    sectionNumber: 1,
    totalSections: 10,
    primaryAnswerId: null,
    secondaryAnswerId: null,
    onSelectPrimary: vi.fn(),
    onSelectSecondary: vi.fn(),
    onNext: vi.fn(),
    onBack: vi.fn(),
    isFirst: true,
    isLast: false,
    ...overrides,
  }
  renderWithLocale(<QuestionStep {...props} />)
  return props
}

describe('QuestionStep', () => {
  it('renders the question text and all answer options', () => {
    renderStep()

    expect(
      screen.getByRole('heading', { name: /how do you prefer to start/i }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Option A' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Option D' })).toBeInTheDocument()
  })

  it('disables the advance button until a primary answer is chosen', () => {
    renderStep({ primaryAnswerId: null })

    expect(screen.getByRole('button', { name: /next/i })).toBeDisabled()
  })

  it('treats the first tapped option as the primary answer', async () => {
    const user = userEvent.setup()
    const props = renderStep()

    await user.click(screen.getByRole('button', { name: 'Option B' }))

    expect(props.onSelectPrimary).toHaveBeenCalledWith('b')
  })

  it('treats a second tapped option as the optional secondary answer', async () => {
    const user = userEvent.setup()
    const props = renderStep({ primaryAnswerId: 'a' })

    await user.click(screen.getByRole('button', { name: 'Option C' }))

    expect(props.onSelectSecondary).toHaveBeenCalledWith('c')
  })

  it('undoes the primary answer when its card is tapped again', async () => {
    const user = userEvent.setup()
    const props = renderStep({ primaryAnswerId: 'a' })

    await user.click(screen.getByRole('button', { name: /option a/i }))

    expect(props.onSelectPrimary).toHaveBeenCalledWith(null)
  })

  it('labels the advance button "Submit" on the last question and calls onNext', async () => {
    const user = userEvent.setup()
    const props = renderStep({ primaryAnswerId: 'a', isLast: true })

    const submit = screen.getByRole('button', { name: /submit/i })
    expect(submit).toBeEnabled()

    await user.click(submit)

    expect(props.onNext).toHaveBeenCalledTimes(1)
  })

  it('shows the sending label and disables the button while submitting', () => {
    renderStep({ primaryAnswerId: 'a', isLast: true, submitting: true })

    const button = screen.getByRole('button', { name: /sending/i })
    expect(button).toBeDisabled()
    expect(screen.queryByRole('button', { name: /^submit/i })).not.toBeInTheDocument()
  })
})
