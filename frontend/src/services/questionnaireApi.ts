import type {
  GetMyReflectionResponse,
  GetQuestionnaireResponse,
  SubmitResponseRequest,
  SubmitResponseResult,
} from '../types/api'
import { BASE, ensureOk, parse, withLocale } from './http'

// Re-exported so existing callers keep importing ApiError from here.
export { ApiError } from './http'

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
