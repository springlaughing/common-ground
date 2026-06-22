import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  createInvite,
  getComparison,
  joinInvite,
  listComparisons,
  validateInvite,
} from '../../src/services/comparisonApi'
import { ApiError } from '../../src/services/http'

function jsonResponse(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response
}

function stubFetch(status: number, body: unknown) {
  const fetchMock = vi.fn(async () => jsonResponse(status, body))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

describe('comparisonApi', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('createInvite POSTs the label with credentials and returns the result', async () => {
    const fetchMock = stubFetch(201, { comparisonId: 'c1', inviteToken: 'TOK', expiresAt: 'x', status: 'pending' })

    const result = await createInvite('Alex')

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(String(url)).toContain('/api/comparisons')
    expect(init.method).toBe('POST')
    expect(JSON.parse(init.body as string)).toEqual({ inviterLabel: 'Alex' })
    expect(init.credentials).toBe('include')
    expect(result.inviteToken).toBe('TOK')
  })

  it('validateInvite POSTs the token (no credentials needed)', async () => {
    const fetchMock = stubFetch(200, { inviterLabel: 'Alex', status: 'active', questionnaireVersion: '1.0' })

    const result = await validateInvite('TOK')

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(String(url)).toContain('/api/invite/validate')
    expect(JSON.parse(init.body as string)).toEqual({ token: 'TOK' })
    expect(result.inviterLabel).toBe('Alex')
  })

  it('joinInvite POSTs the full command with credentials', async () => {
    const fetchMock = stubFetch(201, { privateResultLink: '/me#X', accessCode: 'A-B-C', comparisonId: 'c1' })

    const result = await joinInvite({ token: 'TOK', consent: true, inviteeLabel: 'Sam', answers: [] })

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(String(url)).toContain('/api/invite/join')
    expect(init.credentials).toBe('include')
    expect(JSON.parse(init.body as string)).toMatchObject({ token: 'TOK', consent: true, inviteeLabel: 'Sam' })
    expect(result.privateResultLink).toBe('/me#X')
  })

  it('listComparisons GETs the hub list and unwraps comparisons', async () => {
    const items = [{ comparisonId: 'c1', otherLabel: 'Sam', status: 'complete', createdAt: 'x' }]
    const fetchMock = stubFetch(200, { comparisons: items })

    const result = await listComparisons()

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(String(url)).toContain('/api/me/comparisons')
    expect(init.credentials).toBe('include')
    expect(result).toEqual(items)
  })

  it('getComparison appends the locale when provided', async () => {
    const fetchMock = stubFetch(200, { otherLabel: 'Sam', groups: [] })

    await getComparison('c1', 'de')

    expect(String(fetchMock.mock.calls[0][0])).toContain('/api/me/comparisons/c1?locale=de')
  })

  it('getComparison omits the locale query when none is given', async () => {
    const fetchMock = stubFetch(200, { state: 'pending' })

    const result = await getComparison('c1')

    expect(String(fetchMock.mock.calls[0][0])).toMatch(/\/api\/me\/comparisons\/c1$/)
    expect(result).toEqual({ state: 'pending' })
  })

  it('throws an ApiError carrying the backend error code on a non-2xx response', async () => {
    stubFetch(400, { error: 'invalid_label', message: 'A label is required.' })

    await expect(createInvite('')).rejects.toMatchObject({ status: 400, code: 'invalid_label' })
    await expect(createInvite('')).rejects.toBeInstanceOf(ApiError)
  })
})
