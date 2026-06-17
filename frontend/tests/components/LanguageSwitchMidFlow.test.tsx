import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from '../../src/App'
import type { GetQuestionnaireResponse } from '../../src/types/api'
import { LOCALE_STORAGE_KEY } from '../../src/i18n/locales'

// Same questionnaire in both locales: identical IDs and order, only text differs —
// which is exactly what lets in-progress selections (keyed by stable option IDs)
// survive a mid-flow language switch.
const EN: GetQuestionnaireResponse = {
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

const DE: GetQuestionnaireResponse = {
  id: 'v1',
  versionNumber: '1.0',
  questions: [
    {
      id: 'q1', text: 'Erste Frage?', sectionIndex: 1, orderIndex: 1,
      answerOptions: [
        { id: 'q1a', text: 'Möglichkeit A', orderIndex: 1 },
        { id: 'q1b', text: 'Möglichkeit B', orderIndex: 2 },
      ],
    },
    {
      id: 'q2', text: 'Zweite Frage?', sectionIndex: 2, orderIndex: 2,
      answerOptions: [
        { id: 'q2a', text: 'Möglichkeit C', orderIndex: 1 },
        { id: 'q2b', text: 'Möglichkeit D', orderIndex: 2 },
      ],
    },
  ],
}

function jsonResponse(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response
}

describe('App — US2 mid-flow language switch', () => {
  beforeEach(() => localStorage.removeItem(LOCALE_STORAGE_KEY))
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('preserves selected answers and position when switching language mid-flow, and re-renders in the new language', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input)
      if (url.includes('/api/questionnaire/current')) {
        return jsonResponse(200, url.includes('locale=de') ? DE : EN)
      }
      return jsonResponse(404, { error: 'not_found', message: 'not found' })
    })
    vi.stubGlobal('fetch', fetchMock)

    const user = userEvent.setup()
    render(<App />)

    // Walk into the questionnaire (English by default).
    await user.click(screen.getByRole('button', { name: /Get started/i }))
    await user.click(screen.getByRole('checkbox'))
    await user.click(screen.getByRole('button', { name: /Begin/i }))

    // Q1 (English): pick Option A, advance to Q2.
    expect(await screen.findByText('First question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option A/i }))
    await user.click(screen.getByRole('button', { name: /Next/i }))

    // Q2 (English): pick Option C, then switch to German mid-flow.
    expect(await screen.findByText('Second question?')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Option C/i }))
    await user.click(screen.getByRole('button', { name: 'Deutsch' }))

    // Position unchanged: still on Q2, now in German (not bounced back to Q1).
    expect(await screen.findByText('Zweite Frage?')).toBeInTheDocument()
    expect(screen.queryByText('Erste Frage?')).not.toBeInTheDocument()

    // Selection preserved across the switch: Q2's choice (option id q2a) stays pressed.
    expect(screen.getByRole('button', { name: /Möglichkeit C/i })).toHaveAttribute('aria-pressed', 'true')

    // The switch is announced to assistive tech via a live region.
    expect(screen.getByRole('status')).toHaveTextContent(/Deutsch/)

    // Re-fetch used the new locale; the earlier selection on Q1 also survives.
    expect(fetchMock.mock.calls.some(([u]) => String(u).includes('locale=de'))).toBe(true)
    await user.click(screen.getByRole('button', { name: /Zurück/i }))
    expect(await screen.findByText('Erste Frage?')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Möglichkeit A/i })).toHaveAttribute('aria-pressed', 'true')
  })
})
