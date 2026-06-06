import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MeReflection } from '../../src/pages/ReflectionPage/MeReflection'
import type { GetMyReflectionResponse } from '../../src/types/api'

const REFLECTION: GetMyReflectionResponse = {
  reflection: {
    groups: [
      {
        id: 'g1',
        title: 'How you plan',
        insights: [
          { dimensionId: 'd1', title: 'Planning insight', text: 'You plan ahead.', strength: 4 },
        ],
      },
    ],
  },
  accessCodeAvailable: true,
}

function jsonResponse(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response
}

describe('MeReflection (/me route)', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    window.location.hash = ''
  })

  it('exchanges the fragment token for a session and renders the grouped reflection', async () => {
    window.location.hash = '#tok123'
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/session/start')) return jsonResponse(200, {})
      if (url.includes('/api/me/reflection')) return jsonResponse(200, REFLECTION)
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<MeReflection />)

    expect(await screen.findByText('How you plan')).toBeInTheDocument()
    expect(screen.getByText('Planning insight')).toBeInTheDocument()

    // session/start received the raw token from the fragment...
    const start = fetchMock.mock.calls.find(([u]) => String(u).includes('/api/session/start'))
    expect(start).toBeDefined()
    const init = (start?.[1] ?? {}) as RequestInit
    expect(JSON.parse(init.body as string)).toEqual({ token: 'tok123' })

    // ...and the token is scrubbed from the URL once the session is established.
    expect(window.location.hash).toBe('')
  })

  it('shows a "not available" message when the session is rejected (401)', async () => {
    window.location.hash = '#bad-token'
    const fetchMock = vi.fn(async () => jsonResponse(401, { error: 'invalid_token' }))
    vi.stubGlobal('fetch', fetchMock)

    render(<MeReflection />)

    expect(await screen.findByText(/not available/i)).toBeInTheDocument()
  })

  it('falls back to the session cookie when no token is in the fragment', async () => {
    // No hash — a returning user whose link was already scrubbed but who still has the cookie.
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/me/reflection')) return jsonResponse(200, REFLECTION)
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<MeReflection />)

    expect(await screen.findByText('How you plan')).toBeInTheDocument()
    // session/start is skipped when there's no token to exchange.
    expect(fetchMock.mock.calls.some(([u]) => String(u).includes('/api/session/start'))).toBe(false)
  })
})
