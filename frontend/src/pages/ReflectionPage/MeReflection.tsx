import { useEffect, useState } from 'react'
import { ReflectionPage } from './ReflectionPage'
import { ComparisonPage } from '../ComparisonPage/ComparisonPage'
import { ApiError, fetchMyReflection, startSession } from '../../services/questionnaireApi'
import { getComparison, listComparisons } from '../../services/comparisonApi'
import { InviteCreate } from '../../components/InviteCreate/InviteCreate'
import { ComparisonList } from '../../components/ComparisonList/ComparisonList'
import { LanguageProvider, useLanguage } from '../../i18n/LanguageContext'
import { LanguageSwitcher } from '../../components/LanguageSwitcher/LanguageSwitcher'
import { useMessages } from '../../i18n/useMessages'
import type { ComparisonDto, ComparisonListItem, ReflectionDto } from '../../types/api'
import styles from './MeReflection.module.css'

type Status = 'loading' | 'ready' | 'unavailable' | 'error'

// 401 (no/invalid session) and 404 (soft-deleted) both mean "no result to show";
// anything else is an unexpected error.
function statusForLoadError(e: unknown): Status {
  return e instanceof ApiError && (e.status === 401 || e.status === 404) ? 'unavailable' : 'error'
}

/** The `/me` route and hub: a returning user lands here from their saved private result link.
 *  Beyond their own reflection, it lists their comparisons (US4) and lets them open a report or
 *  invite someone new. The token rides in the URL fragment (`/me#TOKEN`); we exchange it for a
 *  session cookie via /session/start, then load via the cookie. */
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
  const [comparisons, setComparisons] = useState<ComparisonListItem[]>([])
  const [viewingId, setViewingId] = useState<string | null>(null)
  const [report, setReport] = useState<ComparisonDto | null>(null)

  // Exchange the fragment token for a session cookie once, on mount. The token rides in the URL
  // fragment (/me#TOKEN); we read it into a local — never into state — and scrub it from history
  // as soon as the session exists.
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

  // Once the session exists, load the reflection in the active locale — re-fetching when the locale
  // changes so a saved reflection re-renders in the viewer's language (US4).
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

  // The comparison list (labels + status — locale-independent). Re-loaded when an invite is created
  // (the panel closes) so a freshly minted pending comparison appears.
  useEffect(() => {
    if (!sessionReady || comparing) return
    let cancelled = false
    listComparisons()
      .then(list => { if (!cancelled) setComparisons(list) })
      .catch(() => { /* the hub list is non-critical; leave it empty on failure */ })
    return () => { cancelled = true }
  }, [sessionReady, comparing])

  // Load the opened comparison's report; re-fetch on locale change so it re-renders in the new
  // language at view time. A not-yet-ready marker leaves the report null (handled below).
  useEffect(() => {
    if (viewingId === null) return
    let cancelled = false
    getComparison(viewingId, locale)
      .then(res => { if (!cancelled) setReport('groups' in res ? res : null) })
      .catch(() => { if (!cancelled) setReport(null) })
    return () => { cancelled = true }
  }, [viewingId, locale])

  function closeReport() {
    setViewingId(null)
    setReport(null)
  }

  if (viewingId !== null && report) {
    return <ComparisonPage comparison={report} onBack={closeReport} languageSwitcher={<LanguageSwitcher />} />
  }

  if (status === 'ready' && reflection) {
    return (
      <>
        <ReflectionPage
          reflection={reflection}
          languageSwitcher={<LanguageSwitcher />}
          onCompare={() => setComparing(true)}
          hubContent={<ComparisonList comparisons={comparisons} onOpen={setViewingId} />}
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
