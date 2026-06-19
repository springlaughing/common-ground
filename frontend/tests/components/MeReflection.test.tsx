import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
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
    // The switch test persists a locale to localStorage — clear it so test order
    // doesn't bleed a non-default locale into the others.
    localStorage.clear()
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

  // ─── T038 (US4) — switch language at view time, re-fetch & re-render ──────────

  it('re-fetches and re-renders the saved reflection in German when the language is switched', async () => {
    // The same reflection in either locale: identical structure, only display text differs
    // (no locale is stored server-side — it is re-assembled per request).
    const localized = (locale: 'en' | 'de'): GetMyReflectionResponse => ({
      reflection: {
        groups: [
          {
            id: 'g1',
            title: locale === 'de' ? 'Wie du planst' : 'How you plan',
            insights: [
              {
                dimensionId: 'd1',
                title: locale === 'de' ? 'Planungseinblick' : 'Planning insight',
                text: locale === 'de' ? 'Du planst voraus.' : 'You plan ahead.',
                strength: 4,
              },
            ],
          },
        ],
      },
      accessCodeAvailable: true,
    })

    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/session/start')) return jsonResponse(200, {})
      if (url.includes('/api/me/reflection')) {
        return jsonResponse(200, localized(url.includes('locale=de') ? 'de' : 'en'))
      }
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<MeReflection />)

    // Loads in English first (the default locale).
    expect(await screen.findByText('How you plan')).toBeInTheDocument()

    // Switch to German via the on-page switcher → re-fetch with locale=de and re-render.
    await user.click(screen.getByRole('button', { name: 'Deutsch' }))

    expect(await screen.findByText('Wie du planst')).toBeInTheDocument()
    expect(screen.getByText('Planungseinblick')).toBeInTheDocument()
    expect(screen.queryByText('How you plan')).not.toBeInTheDocument()

    // The saved reflection was re-fetched in the new locale.
    expect(fetchMock.mock.calls.some(
      ([u]) => String(u).includes('/api/me/reflection') && String(u).includes('locale=de'),
    )).toBe(true)
  })
})
