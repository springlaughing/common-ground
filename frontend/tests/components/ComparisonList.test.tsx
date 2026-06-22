import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { renderWithLocale } from '../support/renderWithLocale'
import { ComparisonList } from '../../src/components/ComparisonList/ComparisonList'
import type { ComparisonListItem } from '../../src/types/api'

const items: ComparisonListItem[] = [
  { comparisonId: 'c1', otherLabel: 'Sam', status: 'complete', createdAt: '2026-06-22T00:00:00Z' },
  { comparisonId: 'c2', otherLabel: 'Jordan', status: 'pending', createdAt: '2026-06-22T00:00:00Z' },
]

describe('ComparisonList', () => {
  it('shows the empty state when there are no comparisons', () => {
    renderWithLocale(<ComparisonList comparisons={[]} onOpen={() => {}} />)

    expect(screen.getByText(/no comparisons yet/i)).toBeInTheDocument()
  })

  it('opens a ready comparison and shows a pending one as waiting (no open button)', () => {
    renderWithLocale(<ComparisonList comparisons={items} onOpen={() => {}} />)

    expect(screen.getByText('Sam')).toBeInTheDocument()
    expect(screen.getByText('Jordan')).toBeInTheDocument()
    // Pending shows a status, not a View action.
    expect(screen.getByText(/waiting for them to join/i)).toBeInTheDocument()
    // Exactly one openable (complete) comparison.
    expect(screen.getAllByRole('button', { name: /view/i })).toHaveLength(1)
  })

  it('calls onOpen with the comparison id when View is clicked', async () => {
    const onOpen = vi.fn()
    const user = userEvent.setup()
    renderWithLocale(<ComparisonList comparisons={items} onOpen={onOpen} />)

    await user.click(screen.getByRole('button', { name: /view/i }))

    expect(onOpen).toHaveBeenCalledWith('c1')
  })
})
