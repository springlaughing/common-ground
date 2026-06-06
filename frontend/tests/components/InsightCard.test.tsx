import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { InsightCard } from '../../src/components/InsightCard/InsightCard'

describe('InsightCard', () => {
  it('renders the insight title and text', () => {
    render(<InsightCard title="Planning insight" text="You plan ahead." strength={3} />)

    expect(screen.getByText('Planning insight')).toBeInTheDocument()
    expect(screen.getByText('You plan ahead.')).toBeInTheDocument()
  })

  it.each([1, 2, 3, 4, 5])('fills exactly %i of 5 dots for strength %i', strength => {
    const { container } = render(<InsightCard title="t" text="x" strength={strength} />)

    expect(container.querySelectorAll('[data-filled="true"]')).toHaveLength(strength)
    expect(container.querySelectorAll('[data-filled="false"]')).toHaveLength(5 - strength)
    // The strength is also exposed accessibly.
    expect(screen.getByRole('img')).toHaveAttribute('aria-label', `Strength ${strength} out of 5`)
  })
})
