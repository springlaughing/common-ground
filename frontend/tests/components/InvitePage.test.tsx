import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { InvitePage } from '../../src/pages/InvitePage/InvitePage'
import { joinInvite, validateInvite } from '../../src/services/comparisonApi'
import { fetchCurrentQuestionnaire } from '../../src/services/questionnaireApi'
import type { GetQuestionnaireResponse } from '../../src/types/api'

vi.mock('../../src/services/comparisonApi', () => ({
  validateInvite: vi.fn(),
  joinInvite: vi.fn(),
}))
vi.mock('../../src/services/questionnaireApi', () => ({
  fetchCurrentQuestionnaire: vi.fn(),
}))

const mockedValidate = vi.mocked(validateInvite)
const mockedJoin = vi.mocked(joinInvite)
const mockedFetch = vi.mocked(fetchCurrentQuestionnaire)

const ACTIVE = { inviterLabel: 'Alex', status: 'active', questionnaireVersion: '1.0' }

const ONE_QUESTION: GetQuestionnaireResponse = {
  id: 'v1',
  versionNumber: '1.0',
  questions: [
    {
      id: 'q1', text: 'First question?', sectionIndex: 1, orderIndex: 1,
      answerOptions: [
        { id: 'q1a', text: 'Option A', orderIndex: 1 },
        { id: 'q1b', text: 'Option B', orderIndex: 2 },
      ],
    },
  ],
}

describe('InvitePage (/invite route)', () => {
  beforeEach(() => {
    mockedValidate.mockReset()
    mockedJoin.mockReset()
    mockedFetch.mockReset()
    window.location.hash = '#invite-token'
  })
  afterEach(() => {
    window.location.hash = ''
    localStorage.clear()
  })

  it('shows the neutral invalid state when there is no token (and never calls the API)', async () => {
    window.location.hash = ''
    render(<InvitePage />)

    expect(await screen.findByText('Invite not available')).toBeInTheDocument()
    expect(mockedValidate).not.toHaveBeenCalled()
  })

  it('shows the invalid state when the invite cannot be validated', async () => {
    mockedValidate.mockRejectedValue(new Error('gone'))
    render(<InvitePage />)

    expect(await screen.findByText('Invite not available')).toBeInTheDocument()
  })

  it('validates without consuming, then shows §V consent: inviter label, label disclosure, equal-weight accept/decline', async () => {
    mockedValidate.mockResolvedValue(ACTIVE)
    render(<InvitePage />)

    expect(await screen.findByText(/Alex has invited you/i)).toBeInTheDocument()
    // Discloses that the chosen label is shared with the inviter.
    expect(screen.getByText(/shown to Alex/i)).toBeInTheDocument()
    // Equal weight: accept and decline are both buttons (decline is not a hidden/secondary link).
    expect(screen.getByRole('button', { name: /Yes, I’ll join/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /No, thanks/i })).toBeInTheDocument()
    // Validation must not consume the invite.
    expect(mockedJoin).not.toHaveBeenCalled()
  })

  it('declining creates nothing', async () => {
    mockedValidate.mockResolvedValue(ACTIVE)
    const user = userEvent.setup()
    render(<InvitePage />)

    await screen.findByText(/Alex has invited you/i)
    await user.click(screen.getByRole('button', { name: /No, thanks/i }))

    expect(await screen.findByText('No problem')).toBeInTheDocument()
    expect(mockedJoin).not.toHaveBeenCalled()
  })

  it('accepting proceeds to the questionnaire and joining returns the invitee’s own credentials', async () => {
    mockedValidate.mockResolvedValue(ACTIVE)
    mockedFetch.mockResolvedValue(ONE_QUESTION)
    mockedJoin.mockResolvedValue({ privateResultLink: '/me#INVITEE', accessCode: 'K7Q9-MP2D-W4T8', comparisonId: 'c1' })
    const user = userEvent.setup()
    render(<InvitePage />)

    // Consent → enter own label → accept.
    await screen.findByText(/Alex has invited you/i)
    await user.type(screen.getByLabelText(/your name or label/i), 'Sam')
    await user.click(screen.getByRole('button', { name: /Yes, I’ll join/i }))

    // The same questionnaire — answer it.
    expect(await screen.findByText('First question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option A/i }))
    await user.click(screen.getByRole('button', { name: /Submit/i }))

    // Joined → own credentials shown; join called with consent + label + token.
    expect(await screen.findByText('You’re in')).toBeInTheDocument()
    expect(screen.getByText(/\/me#INVITEE/)).toBeInTheDocument()
    expect(mockedJoin).toHaveBeenCalledWith(expect.objectContaining({
      token: 'invite-token',
      consent: true,
      inviteeLabel: 'Sam',
    }))
  })
})
