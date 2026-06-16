import { screen } from '@testing-library/react'
import { renderWithLocale } from '../support/renderWithLocale'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ReflectionPage } from '../../src/pages/ReflectionPage/ReflectionPage'
import type { ReflectionDto } from '../../src/types/api'

const REFLECTION: ReflectionDto = {
  groups: [
    {
      id: 'g1',
      title: 'How you plan',
      insights: [
        { dimensionId: 'd1', title: 'Planning insight', text: 'You plan ahead.', strength: 4 },
      ],
    },
    {
      id: 'g2',
      title: 'How you communicate',
      insights: [
        { dimensionId: 'd2', title: 'Written context', text: 'You prefer docs.', strength: 5 },
      ],
    },
  ],
}

describe('ReflectionPage', () => {
  it('renders every group title and its insights', () => {
    renderWithLocale(<ReflectionPage reflection={REFLECTION} />)

    expect(screen.getByText('How you plan')).toBeInTheDocument()
    expect(screen.getByText('How you communicate')).toBeInTheDocument()
    expect(screen.getByText('Planning insight')).toBeInTheDocument()
    expect(screen.getByText('You prefer docs.')).toBeInTheDocument()
  })

  it('shows the compare CTA only when an onCompare handler is provided', async () => {
    const onCompare = vi.fn()
    const { rerender } = renderWithLocale(<ReflectionPage reflection={REFLECTION} />)
    expect(screen.queryByRole('button', { name: /compare/i })).not.toBeInTheDocument()

    rerender(<ReflectionPage reflection={REFLECTION} onCompare={onCompare} />)
    await userEvent.click(screen.getByRole('button', { name: /compare/i }))
    expect(onCompare).toHaveBeenCalledOnce()
  })
})
