import type { CreateInviteResult } from '../types/api'
import { BASE, parse } from './http'

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
