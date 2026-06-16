import { screen } from '@testing-library/react'
import { renderWithLocale } from '../support/renderWithLocale'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ConsentStep } from '../../src/components/ConsentStep/ConsentStep'

describe('ConsentStep', () => {
  it('keeps the "Begin" button disabled until the user acknowledges', () => {
    renderWithLocale(<ConsentStep onAcknowledge={vi.fn()} onBack={vi.fn()} />)

    expect(screen.getByRole('button', { name: /begin/i })).toBeDisabled()
  })

  it('enables "Begin" once the consent checkbox is checked', async () => {
    const user = userEvent.setup()
    renderWithLocale(<ConsentStep onAcknowledge={vi.fn()} onBack={vi.fn()} />)

    await user.click(screen.getByRole('checkbox'))

    expect(screen.getByRole('button', { name: /begin/i })).toBeEnabled()
  })

  it('calls onAcknowledge after the user agrees and clicks Begin', async () => {
    const user = userEvent.setup()
    const onAcknowledge = vi.fn()
    renderWithLocale(<ConsentStep onAcknowledge={onAcknowledge} onBack={vi.fn()} />)

    await user.click(screen.getByRole('checkbox'))
    await user.click(screen.getByRole('button', { name: /begin/i }))

    expect(onAcknowledge).toHaveBeenCalledTimes(1)
  })
})
