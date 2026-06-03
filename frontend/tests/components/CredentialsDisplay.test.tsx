import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { CredentialsDisplay } from '../../src/components/CredentialsDisplay/CredentialsDisplay'

const ACCESS_CODE = 'K7Q9-MP2D-W4T8'

describe('CredentialsDisplay', () => {
  it('shows both credentials with distinct labels and explanations', () => {
    render(<CredentialsDisplay privateResultLink="/me#TOKEN" accessCode={ACCESS_CODE} />)

    expect(screen.getByText(/your private result link/i)).toBeInTheDocument()
    expect(screen.getByText(/bookmark this to return/i)).toBeInTheDocument()

    expect(screen.getByText(/your access code/i)).toBeInTheDocument()
    expect(
      screen.getByText(/reuse your response in a future comparison/i),
    ).toBeInTheDocument()
    expect(screen.getByText(ACCESS_CODE)).toBeInTheDocument()
  })

  it('warns that the access code must be kept private', () => {
    render(<CredentialsDisplay privateResultLink="/me#TOKEN" accessCode={ACCESS_CODE} />)

    expect(screen.getByText(/keep it private/i)).toBeInTheDocument()
  })

  it('copies the access code to the clipboard', async () => {
    const user = userEvent.setup()
    const writeText = vi.fn().mockResolvedValue(undefined)
    Object.defineProperty(navigator, 'clipboard', {
      value: { writeText },
      configurable: true,
    })

    render(<CredentialsDisplay privateResultLink="/me#TOKEN" accessCode={ACCESS_CODE} />)
    await user.click(screen.getByRole('button', { name: /copy access code/i }))

    expect(writeText).toHaveBeenCalledWith(ACCESS_CODE)
  })
})
