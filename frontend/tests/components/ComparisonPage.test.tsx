import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithLocale } from '../support/renderWithLocale'
import { ComparisonPage } from '../../src/pages/ComparisonPage/ComparisonPage'
import type { ComparisonDto } from '../../src/types/api'

const REPORT: ComparisonDto = {
  otherLabel: 'Sam',
  groups: [
    {
      id: 'g1',
      title: 'Group One',
      insights: [
        {
          dimensionId: 'd1', title: 'A clear difference',
          yourStrength: 5, theirStrength: 1, yourText: 'you on d1', theirText: 'them on d1',
          classification: 'difference',
        },
        {
          dimensionId: 'd2', title: 'A shared strength',
          yourStrength: 3, theirStrength: 3, yourText: 'shared text', theirText: 'shared text (them)',
          classification: 'similarity',
        },
      ],
    },
  ],
}

describe('ComparisonPage', () => {
  it('renders differences and similarities under their own headings, differences first', () => {
    renderWithLocale(<ComparisonPage comparison={REPORT} />)

    const headings = screen.getAllByRole('heading', { level: 2 }).map(h => h.textContent)
    expect(headings).toEqual(['Where you differ', 'Where you align'])

    expect(screen.getByText('A clear difference')).toBeInTheDocument()
    expect(screen.getByText('A shared strength')).toBeInTheDocument()
  })

  it('names the other person and the viewer, and shows a neutral count (no compatibility score)', () => {
    renderWithLocale(<ComparisonPage comparison={REPORT} />)

    // Per-viewer: the other person is named; "you" is the viewer.
    expect(screen.getAllByText('Sam').length).toBeGreaterThan(0)
    expect(screen.getAllByText('You').length).toBeGreaterThan(0)

    // Neutral per-dimension count, not a score.
    expect(screen.getByText('You align on 1 of 2 shown dimensions.')).toBeInTheDocument()
    expect(screen.queryByText(/score|compatib|%/i)).not.toBeInTheDocument()
  })

  it('renders the report chrome in the active language (German)', () => {
    renderWithLocale(<ComparisonPage comparison={REPORT} />, 'de')

    expect(screen.getByText('Wo ihr euch unterscheidet')).toBeInTheDocument()
    expect(screen.getByText('Wo ihr ähnlich tickt')).toBeInTheDocument()
    expect(screen.getByText('Ihr seid bei 1 von 2 gezeigten Dimensionen ähnlich.')).toBeInTheDocument()
  })
})
