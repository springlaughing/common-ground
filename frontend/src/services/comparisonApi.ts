import type {
  ComparisonListItem,
  ComparisonReportResponse,
  CreateInviteResult,
  InviteValidation,
  JoinInviteRequest,
  JoinInviteResult,
} from '../types/api'
import { BASE, parse, withLocale } from './http'

/** US1 (T016) — the inviter mints a single-use, time-limited invite for their response.
 *  Requires the cg_session cookie (the inviter's session). Returns the plain invite token
 *  the caller turns into a shareable `/invite#<token>` link. */
export async function createInvite(inviterLabel: string): Promise<CreateInviteResult> {
  const res = await fetch(`${BASE}/comparisons`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inviterLabel }),
    credentials: 'include',
  })
  return parse<CreateInviteResult>(res)
}

/** US2 (T025) — read an invite (from the link fragment) without consuming it, so the consent
 *  screen can show the inviter's label. No session required. */
export async function validateInvite(token: string): Promise<InviteValidation> {
  const res = await fetch(`${BASE}/invite/validate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token }),
  })
  return parse<InviteValidation>(res)
}

/** US2 (T025) — the invitee consents and submits their answers; consumes the invite single-use,
 *  creates their own response, and starts their session (sets the cg_session cookie). Returns the
 *  invitee's own private link + access code. */
export async function joinInvite(request: JoinInviteRequest): Promise<JoinInviteResult> {
  const res = await fetch(`${BASE}/invite/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    credentials: 'include',
  })
  return parse<JoinInviteResult>(res)
}

/** US4 (T038) — every comparison the current session is part of (the /me hub). Requires the cg_session cookie. */
export async function listComparisons(): Promise<ComparisonListItem[]> {
  const res = await fetch(`${BASE}/me/comparisons`, { credentials: 'include' })
  const body = await parse<{ comparisons: ComparisonListItem[] }>(res)
  return body.comparisons
}

/** US4 (T038) — one comparison report from the caller's perspective, in the given locale. Returns the
 *  report when ready, or a `{ state }` marker for a pending/unavailable comparison. */
export async function getComparison(comparisonId: string, locale?: string): Promise<ComparisonReportResponse> {
  const res = await fetch(withLocale(`${BASE}/me/comparisons/${comparisonId}`, locale), { credentials: 'include' })
  return parse<ComparisonReportResponse>(res)
}
