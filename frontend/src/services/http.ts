// Shared fetch plumbing for the API service modules. Keeping it in one place avoids
// duplicating the error-shape handling across questionnaireApi / comparisonApi (and the
// duplication Sonar would otherwise flag).

// Same-origin base path; Vite proxies /api to the backend in dev (see vite.config.ts),
// and a reverse proxy will do the same in production.
export const BASE = '/api'

/** Appends `?locale=` to a content endpoint when a locale is provided. Omitting it
 *  lets the backend default to English (SupportedLocales). */
export function withLocale(path: string, locale?: string): string {
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
export async function ensureOk(res: Response): Promise<void> {
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

export async function parse<T>(res: Response): Promise<T> {
  await ensureOk(res)
  // 201/200 bodies are JSON; 204 (no content) returns undefined.
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}
