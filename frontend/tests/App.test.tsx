import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import App from '../src/App'
import type { GetQuestionnaireResponse, SubmitResponseResult } from '../src/types/api'

// Minimal two-question questionnaire so the test stays fast and readable.
const QUESTIONNAIRE: GetQuestionnaireResponse = {
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
    {
      id: 'q2', text: 'Second question?', sectionIndex: 2, orderIndex: 2,
      answerOptions: [
        { id: 'q2a', text: 'Option C', orderIndex: 1 },
        { id: 'q2b', text: 'Option D', orderIndex: 2 },
      ],
    },
  ],
}

const RESULT: SubmitResponseResult = {
  privateResultLink: '/me#tok123',
  accessCode: 'ABCD-1234-WXYZ',
  reflection: {
    groups: [
      {
        id: 'g1', title: 'How you plan',
        insights: [{ dimensionId: 'd1', title: 'Planning insight', strength: 4, text: 'You plan ahead.' }],
      },
    ],
  },
}

function jsonResponse(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response
}

describe('App — US1 questionnaire flow wired to the API', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('loads questions from the API, submits real answers, and renders the returned credentials + reflection', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input)
      if (url.includes('/api/questionnaire/current')) return jsonResponse(200, QUESTIONNAIRE)
      if (url.includes('/api/responses') && init?.method === 'POST') return jsonResponse(201, RESULT)
      return jsonResponse(404, { error: 'not_found', message: 'not found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<App />)

    // Welcome → Consent → start
    await user.click(screen.getByRole('button', { name: /Get started/i }))
    await user.click(screen.getByRole('checkbox'))
    await user.click(screen.getByRole('button', { name: /Begin/i }))

    // Question 1 (loaded from the API, not a hardcoded mock)
    expect(await screen.findByText('First question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option A/i }))
    await user.click(screen.getByRole('button', { name: /Next/i }))

    // Question 2 (last) → submit
    expect(await screen.findByText('Second question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option C/i }))
    await user.click(screen.getByRole('button', { name: /Submit/i }))

    // Completion — credentials come from the 201 response, not demo constants
    expect(await screen.findByText('Your reflection is ready.')).toBeInTheDocument()
    expect(screen.getByText('ABCD-1234-WXYZ')).toBeInTheDocument()
    expect(screen.getByText(/\/me#tok123/)).toBeInTheDocument()

    // Reflection — rendered from the returned reflection
    await user.click(screen.getByRole('button', { name: /View my reflection/i }))
    expect(await screen.findByText('How you plan')).toBeInTheDocument()
    expect(screen.getByText('Planning insight')).toBeInTheDocument()

    // The POST carried exactly the answers we selected.
    const post = fetchMock.mock.calls.find(([url]) => String(url).includes('/api/responses'))
    expect(post).toBeDefined()
    const init = (post?.[1] ?? {}) as RequestInit
    const body = JSON.parse(init.body as string)
    expect(body.answers).toEqual([
      { questionId: 'q1', primaryAnswerOptionId: 'q1a' },
      { questionId: 'q2', primaryAnswerOptionId: 'q2a' },
    ])
  })

  it('shows a graceful error (with retry) when the questionnaire fails to load', async () => {
    const fetchMock = vi.fn(async () =>
      jsonResponse(503, { error: 'no_active_questionnaire', message: 'No active questionnaire is available.' }),
    )
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<App />)

    await user.click(screen.getByRole('button', { name: /Get started/i }))
    await user.click(screen.getByRole('checkbox'))
    await user.click(screen.getByRole('button', { name: /Begin/i }))

    expect(await screen.findByText(/No active questionnaire is available\./i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Retry/i })).toBeInTheDocument()
  })
})
