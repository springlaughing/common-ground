import { useEffect, useState } from 'react'
import { ReflectionPage } from './ReflectionPage'
import { ApiError, fetchMyReflection, startSession } from '../../services/questionnaireApi'
import { InviteCreate } from '../../components/InviteCreate/InviteCreate'
import { LanguageProvider, useLanguage } from '../../i18n/LanguageContext'
import { LanguageSwitcher } from '../../components/LanguageSwitcher/LanguageSwitcher'
import { useMessages } from '../../i18n/useMessages'
import type { ReflectionDto } from '../../types/api'
import styles from './MeReflection.module.css'

type Status = 'loading' | 'ready' | 'unavailable' | 'error'

// 401 (no/invalid session) and 404 (soft-deleted) both mean "no result to show";
// anything else is an unexpected error.
function statusForLoadError(e: unknown): Status {
  return e instanceof ApiError && (e.status === 401 || e.status === 404) ? 'unavailable' : 'error'
}

/** The `/me` route: a returning user lands here from their saved private result link.
 *  The token rides in the URL fragment (`/me#TOKEN`). We exchange it for a session
 *  cookie via /session/start, then load the reflection via /me/reflection. If the user
 *  returns without a token (the fragment was already scrubbed) we rely on the existing
 *  cg_session cookie. */
export function MeReflection() {
  return (
    <LanguageProvider>
      <MeReflectionView />
    </LanguageProvider>
  )
}

function MeReflectionView() {
  const { locale } = useLanguage()
  const m = useMessages()
  const [status, setStatus] = useState<Status>('loading')
  const [reflection, setReflection] = useState<ReflectionDto | null>(null)
  const [sessionReady, setSessionReady] = useState(false)
  const [comparing, setComparing] = useState(false)

  // Exchange the fragment token for a session cookie once, on mount. The token rides in
  // the URL fragment (/me#TOKEN); we read it into a local — never into state — so the raw
  // token isn't retained, and scrub it from history as soon as the session exists.
  useEffect(() => {
    let cancelled = false
    async function startIfNeeded() {
      try {
        const token = window.location.hash.replace(/^#/, '')
        if (token) {
          await startSession(token)
          window.history.replaceState(null, '', window.location.pathname)
        }
        if (!cancelled) setSessionReady(true)
      } catch (e) {
        if (!cancelled) setStatus(statusForLoadError(e))
      }
    }
    void startIfNeeded()
    return () => { cancelled = true }
  }, [])

  // Once the session exists, load the reflection in the active locale — re-fetching when
  // the locale changes so a saved reflection re-renders in the viewer's language (US4). No
  // locale is stored server-side; the same insights/strengths come back, re-rendered. The
  // previous reflection stays on screen during the swap (no flash back to "loading").
  useEffect(() => {
    if (!sessionReady) return
    let cancelled = false
    async function loadReflection() {
      try {
        const data = await fetchMyReflection(locale)
        if (cancelled) return
        setReflection(data.reflection)
        setStatus('ready')
      } catch (e) {
        if (!cancelled) setStatus(statusForLoadError(e))
      }
    }
    void loadReflection()
    return () => { cancelled = true }
  }, [sessionReady, locale])

  if (status === 'ready' && reflection) {
    return (
      <>
        <ReflectionPage
          reflection={reflection}
          languageSwitcher={<LanguageSwitcher />}
          onCompare={() => setComparing(true)}
        />
        {comparing && <InviteCreate onClose={() => setComparing(false)} />}
      </>
    )
  }

  return (
    <div className={styles.center}>
      <span className={styles.brand}>common ground</span>

      {status === 'loading' && <p className={styles.message}>{m.me.loading}</p>}

      {status === 'unavailable' && (
        <>
          <h1 className={styles.title}>{m.me.unavailableTitle}</h1>
          <p className={styles.message}>{m.me.unavailableBody}</p>
        </>
      )}

      {status === 'error' && (
        <>
          <p className={styles.message}>{m.me.errorMessage}</p>
          <button className={styles.retry} onClick={() => globalThis.location.reload()}>
            {m.me.retry}
          </button>
        </>
      )}
    </div>
  )
}
