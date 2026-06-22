import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithLocale } from '../support/renderWithLocale'
import { InviteCreate } from '../../src/components/InviteCreate/InviteCreate'
import { createInvite } from '../../src/services/comparisonApi'

vi.mock('../../src/services/comparisonApi', () => ({
  createInvite: vi.fn(),
}))

const mockedCreateInvite = vi.mocked(createInvite)

function invite(token = 'TESTTOKEN') {
  return { comparisonId: 'c1', inviteToken: token, expiresAt: '2026-07-01T00:00:00Z', status: 'pending' }
}

describe('InviteCreate', () => {
  beforeEach(() => {
    mockedCreateInvite.mockReset()
  })

  it('disables creating until a label is entered', () => {
    renderWithLocale(<InviteCreate onClose={() => {}} />)

    expect(screen.getByRole('button', { name: /create invite link/i })).toBeDisabled()
  })

  it('posts the entered label and surfaces the shareable invite link', async () => {
    const user = userEvent.setup()
    mockedCreateInvite.mockResolvedValue(invite())

    renderWithLocale(<InviteCreate onClose={() => {}} />)
    await user.type(screen.getByLabelText(/your name or label/i), 'Alex')
    await user.click(screen.getByRole('button', { name: /create invite link/i }))

    expect(mockedCreateInvite).toHaveBeenCalledWith('Alex')
    expect(await screen.findByText(/your invite link is ready/i)).toBeInTheDocument()
    expect(screen.getByText(/\/invite#TESTTOKEN/)).toBeInTheDocument()
  })

  it('copies the invite link to the clipboard', async () => {
    const user = userEvent.setup()
    const writeText = vi.fn().mockResolvedValue(undefined)
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true })
    mockedCreateInvite.mockResolvedValue(invite())

    renderWithLocale(<InviteCreate onClose={() => {}} />)
    await user.type(screen.getByLabelText(/your name or label/i), 'Alex')
    await user.click(screen.getByRole('button', { name: /create invite link/i }))
    await screen.findByText(/your invite link is ready/i)
    await user.click(screen.getByRole('button', { name: /copy the invite link/i }))

    expect(writeText).toHaveBeenCalledWith(expect.stringContaining('/invite#TESTTOKEN'))
  })

  it('shows an error when creating the invite fails', async () => {
    const user = userEvent.setup()
    mockedCreateInvite.mockRejectedValue(new Error('network'))

    renderWithLocale(<InviteCreate onClose={() => {}} />)
    await user.type(screen.getByLabelText(/your name or label/i), 'Alex')
    await user.click(screen.getByRole('button', { name: /create invite link/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/something went wrong creating your invite/i)
  })
})
