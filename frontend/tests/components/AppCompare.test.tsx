import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import App from '../../src/App'
import type { GetQuestionnaireResponse } from '../../src/types/api'

const ONE_QUESTION: GetQuestionnaireResponse = {
  id: 'v1',
  versionNumber: '1.0',
  questions: [
    {
      id: 'q1', text: 'Only question?', sectionIndex: 1, orderIndex: 1,
      answerOptions: [
        { id: 'q1a', text: 'Option A', orderIndex: 1 },
        { id: 'q1b', text: 'Option B', orderIndex: 2 },
      ],
    },
  ],
}

const RESULT = {
  privateResultLink: '/me#TOK123',
  accessCode: 'K7Q9-MP2D-W4T8',
  reflection: {
    groups: [{ id: 'g1', title: 'How you plan', insights: [{ dimensionId: 'd1', title: 'Plan', text: 'You plan.', strength: 4 }] }],
  },
}

function jsonResponse(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response
}

describe('App — first-time "compare" opens the real invite panel', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    window.location.hash = ''
    localStorage.clear()
  })

  it('completes the questionnaire, then "compare" starts a session and shows InviteCreate', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/questionnaire/current')) return jsonResponse(200, ONE_QUESTION)
      if (url.includes('/api/responses')) return jsonResponse(201, RESULT)
      if (url.includes('/api/session/start')) return jsonResponse(200, {})
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<App />)

    await user.click(screen.getByRole('button', { name: /Get started/i }))
    await user.click(screen.getByRole('checkbox'))
    await user.click(screen.getByRole('button', { name: /Begin/i }))

    expect(await screen.findByText('Only question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option A/i }))
    await user.click(screen.getByRole('button', { name: /Submit/i }))

    // Completion → reflection.
    await user.click(await screen.findByRole('button', { name: /View my reflection/i }))
    expect(await screen.findByText('How you plan')).toBeInTheDocument()

    // "Compare" starts a session from the freshly minted token, then opens the invite panel.
    await user.click(screen.getByRole('button', { name: /Compare with someone/i }))

    expect(await screen.findByText('Invite someone to compare')).toBeInTheDocument()
    const start = fetchMock.mock.calls.find(([u]) => String(u).includes('/api/session/start'))
    expect(start).toBeDefined()
    expect(JSON.parse(((start?.[1] ?? {}) as RequestInit).body as string)).toEqual({ token: 'TOK123' })
  })
})
