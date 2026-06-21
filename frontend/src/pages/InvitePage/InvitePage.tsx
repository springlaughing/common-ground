import { LanguageProvider } from '../../i18n/LanguageContext'
import { LanguageSwitcher } from '../../components/LanguageSwitcher/LanguageSwitcher'
import { useMessages } from '../../i18n/useMessages'
import styles from './InvitePage.module.css'

/** The `/invite` route: an invitee opens their single-use invite link, whose token rides
 *  in the URL fragment (`/invite#TOKEN`) — same privacy property as `/me#TOKEN`.
 *
 *  Phase 1 scaffold (T002): this only establishes the route + localized chrome. It reads
 *  whether a fragment token is present and shows the invite landing (token present) or the
 *  invalid state (no token). The real flow — validate without consuming, §V consent,
 *  questionnaire, then the invitee's own credentials — is built in T026. */
export function InvitePage() {
  return (
    <LanguageProvider>
      <InvitePageView />
    </LanguageProvider>
  )
}

function InvitePageView() {
  const m = useMessages()
  const hasToken = window.location.hash.replace(/^#/, '').length > 0

  return (
    <div className={styles.center}>
      <span className={styles.brand}>common ground</span>
      <LanguageSwitcher />

      {hasToken ? (
        <>
          <span className={styles.eyebrow}>{m.invite.eyebrow}</span>
          <h1 className={styles.title}>{m.invite.title}</h1>
          <p className={styles.message}>{m.invite.intro}</p>
        </>
      ) : (
        <>
          <h1 className={styles.title}>{m.invite.invalidTitle}</h1>
          <p className={styles.message}>{m.invite.invalidBody}</p>
        </>
      )}
    </div>
  )
}
