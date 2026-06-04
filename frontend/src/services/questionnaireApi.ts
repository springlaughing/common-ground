import type {
  GetMyReflectionResponse,
  GetQuestionnaireResponse,
  SubmitResponseRequest,
  SubmitResponseResult,
} from '../types/api'

// Same-origin base path; Vite proxies /api to the backend in dev (see
// vite.config.ts), and a reverse proxy will do the same in production.
const BASE = '/api'

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

async function parse<T>(res: Response): Promise<T> {
  if (!res.ok) {
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
  // 201/200 bodies are JSON; 204 (no content) returns undefined.
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

/** US1 — load the active questionnaire (questions + options, no dimension weights). */
export async function fetchCurrentQuestionnaire(
  signal?: AbortSignal,
): Promise<GetQuestionnaireResponse> {
  const res = await fetch(`${BASE}/questionnaire/current`, { signal })
  return parse<GetQuestionnaireResponse>(res)
}

/** US1 — submit all answers; returns the private result link, access code, and reflection. */
export async function submitResponses(
  request: SubmitResponseRequest,
): Promise<SubmitResponseResult> {
  const res = await fetch(`${BASE}/responses`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    credentials: 'include',
  })
  return parse<SubmitResponseResult>(res)
}

/** US2 — validate a private result token (from the URL fragment) and start a session.
 *  Endpoint arrives with T032; typed here per T016. */
export async function startSession(token: string): Promise<void> {
  const res = await fetch(`${BASE}/session/start`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token }),
    credentials: 'include',
  })
  await parse<void>(res)
}

/** US2 — load the reflection for the current session (requires the cg_session cookie).
 *  Endpoint arrives with T033; typed here per T016. */
export async function fetchMyReflection(): Promise<GetMyReflectionResponse> {
  const res = await fetch(`${BASE}/me/reflection`, { credentials: 'include' })
  return parse<GetMyReflectionResponse>(res)
}
