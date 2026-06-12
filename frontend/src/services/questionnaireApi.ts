import type {
  GetMyReflectionResponse,
  GetQuestionnaireResponse,
  SubmitResponseRequest,
  SubmitResponseResult,
} from '../types/api'

// Same-origin base path; Vite proxies /api to the backend in dev (see
// vite.config.ts), and a reverse proxy will do the same in production.
const BASE = '/api'

/** Appends `?locale=` to a content endpoint when a locale is provided. Omitting it
 *  lets the backend default to English (SupportedLocales). */
function withLocale(path: string, locale?: string): string {
  if (!locale) return path
  const sep = path.includes('?') ? '&' : '?'
  return `${path}${sep}locale=${encodeURIComponent(locale)}`
}

/** Error carrying the HTTP status and the backend's machine-readable error code. */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly code?: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

/** Throw an ApiError carrying the backend's status + machine-readable code for a
 *  non-2xx response. No-op on success, so it's safe to call before reading a body. */
async function ensureOk(res: Response): Promise<void> {
  if (res.ok) return
  let code: string | undefined
  let message = `Request failed (${res.status})`
  try {
    const body = await res.json()
    code = body.error
    if (body.message) message = body.message
  } catch {
    // Non-JSON error body — keep the generic message.
  }
  throw new ApiError(res.status, message, code)
}

async function parse<T>(res: Response): Promise<T> {
  await ensureOk(res)
  // 201/200 bodies are JSON; 204 (no content) returns undefined.
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

/** US1 — load the active questionnaire (questions + options, no dimension weights). */
export async function fetchCurrentQuestionnaire(
  signal?: AbortSignal,
  locale?: string,
): Promise<GetQuestionnaireResponse> {
  const res = await fetch(withLocale(`${BASE}/questionnaire/current`, locale), { signal })
  return parse<GetQuestionnaireResponse>(res)
}

/** US1 — submit all answers; returns the private result link, access code, and reflection. */
export async function submitResponses(
  request: SubmitResponseRequest,
  locale?: string,
): Promise<SubmitResponseResult> {
  const res = await fetch(withLocale(`${BASE}/responses`, locale), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    credentials: 'include',
  })
  return parse<SubmitResponseResult>(res)
}

/** US2 — validate a private result token (from the URL fragment) and start a session.
 *  On success the backend sets the HttpOnly cg_session cookie and returns 200 with an
 *  empty body, so we only verify success — there's nothing to parse. */
export async function startSession(token: string): Promise<void> {
  const res = await fetch(`${BASE}/session/start`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token }),
    credentials: 'include',
  })
  await ensureOk(res)
}

/** US2 — load the reflection for the current session (requires the cg_session cookie).
 *  Endpoint arrives with T033; typed here per T016. */
export async function fetchMyReflection(locale?: string): Promise<GetMyReflectionResponse> {
  const res = await fetch(withLocale(`${BASE}/me/reflection`, locale), { credentials: 'include' })
  return parse<GetMyReflectionResponse>(res)
}
