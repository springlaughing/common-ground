import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MeReflection } from '../../src/pages/ReflectionPage/MeReflection'
import { LOCALE_STORAGE_KEY } from '../../src/i18n/locales'
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

  it('shows the generic error state when re-fetching in a new language fails', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/session/start')) return jsonResponse(200, {})
      if (url.includes('/api/me/reflection')) {
        // English loads fine; the German re-fetch hits a server error.
        return url.includes('locale=de')
          ? jsonResponse(500, { error: 'server_error' })
          : jsonResponse(200, REFLECTION)
      }
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<MeReflection />)

    expect(await screen.findByText('How you plan')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Deutsch' }))

    // A failed re-fetch surfaces the generic error state — distinct from the 401/404
    // "not available" state — so a transient failure on switch isn't read as a dead link.
    // The switch was to German, so the error chrome renders in German too.
    expect(await screen.findByText(/schiefgelaufen/i)).toBeInTheDocument()
    expect(screen.queryByText('Ergebnis nicht verfügbar')).not.toBeInTheDocument()
  })

  it('renders the /me chrome in the active language (German "unavailable" state)', async () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'de')
    window.location.hash = '#bad-token'
    const fetchMock = vi.fn(async () => jsonResponse(401, { error: 'invalid_token' }))
    vi.stubGlobal('fetch', fetchMock)

    render(<MeReflection />)

    // The empty/error chrome localizes too, so it stays consistent with <html lang="de">.
    expect(await screen.findByText('Ergebnis nicht verfügbar')).toBeInTheDocument()
    expect(screen.queryByText(/not available/i)).not.toBeInTheDocument()
  })

  // ─── US4 — the hub lists comparisons and opens a report ──────────────────────

  it('lists the caller’s comparisons and opens a report, then returns to the hub', async () => {
    window.location.hash = '#tok'
    const report = {
      otherLabel: 'Sam',
      groups: [{
        id: 'g1', title: 'Group',
        insights: [{
          dimensionId: 'd1', title: 'A difference',
          yourStrength: 5, theirStrength: 1, yourText: 'you', theirText: 'them',
          classification: 'difference',
        }],
      }],
    }
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/session/start')) return jsonResponse(200, {})
      if (url.includes('/api/me/reflection')) return jsonResponse(200, REFLECTION)
      // Report (has an id segment) before the bare list endpoint.
      if (/\/api\/me\/comparisons\/[^?]+/.test(url)) return jsonResponse(200, report)
      if (url.includes('/api/me/comparisons')) {
        return jsonResponse(200, { comparisons: [{ comparisonId: 'cmp-1', otherLabel: 'Sam', status: 'complete', createdAt: 'x' }] })
      }
      return jsonResponse(404, { error: 'not_found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<MeReflection />)

    // Hub shows the reflection + the comparison list.
    expect(await screen.findByText('How you plan')).toBeInTheDocument()
    await user.click(await screen.findByRole('button', { name: /view/i }))

    // The report renders (per-viewer, differences first).
    expect(await screen.findByText('Where you differ')).toBeInTheDocument()
    expect(screen.getByText('A difference')).toBeInTheDocument()

    // Back returns to the hub.
    await user.click(screen.getByRole('button', { name: /← back/i }))
    expect(await screen.findByText('How you plan')).toBeInTheDocument()
  })
})
