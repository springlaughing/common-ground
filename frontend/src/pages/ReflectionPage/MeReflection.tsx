import { useEffect, useState } from 'react'
import { ReflectionPage } from './ReflectionPage'
import { ApiError, fetchMyReflection, startSession } from '../../services/questionnaireApi'
import type { ReflectionDto } from '../../types/api'
import styles from './MeReflection.module.css'

type Status = 'loading' | 'ready' | 'unavailable' | 'error'

/** The `/me` route: a returning user lands here from their saved private result link.
 *  The token rides in the URL fragment (`/me#TOKEN`). We exchange it for a session
 *  cookie via /session/start, then load the reflection via /me/reflection. If the user
 *  returns without a token (the fragment was already scrubbed) we rely on the existing
 *  cg_session cookie. */
export function MeReflection() {
  const [status, setStatus] = useState<Status>('loading')
  const [reflection, setReflection] = useState<ReflectionDto | null>(null)

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        // Read the token into a local — never into state — so the raw token isn't
        // retained anywhere after we've exchanged it for a session cookie.
        const token = window.location.hash.replace(/^#/, '')
        if (token) {
          await startSession(token)
          // Scrub the token from the address bar and history once the session exists.
          window.history.replaceState(null, '', window.location.pathname)
        }

        const data = await fetchMyReflection()
        if (cancelled) return
        setReflection(data.reflection)
        setStatus('ready')
      } catch (e) {
        if (cancelled) return
        // 401 (no/invalid session) and 404 (soft-deleted) both mean "no result to show".
        setStatus(e instanceof ApiError && (e.status === 401 || e.status === 404)
          ? 'unavailable'
          : 'error')
      }
    }

    void load()
    return () => { cancelled = true }
  }, [])

  if (status === 'ready' && reflection) {
    return <ReflectionPage reflection={reflection} />
  }

  return (
    <div className={styles.center}>
      <span className={styles.brand}>common ground</span>

      {status === 'loading' && <p className={styles.message}>Loading your reflection…</p>}

      {status === 'unavailable' && (
        <>
          <h1 className={styles.title}>Result not available</h1>
          <p className={styles.message}>
            This result link isn’t valid, or the result has been deleted. If you saved a
            private result link, double-check you copied the whole thing.
          </p>
        </>
      )}

      {status === 'error' && (
        <>
          <p className={styles.message}>Something went wrong loading your reflection.</p>
          <button className={styles.retry} onClick={() => globalThis.location.reload()}>
            Retry
          </button>
        </>
      )}
    </div>
  )
}
