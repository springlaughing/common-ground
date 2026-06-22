import { useEffect, useState } from 'react'
import { CredentialsDisplay } from '../../components/CredentialsDisplay/CredentialsDisplay'
import { InviteeConsentStep } from '../../components/ConsentStep/InviteeConsentStep'
import { QuestionnaireFlow } from '../../components/QuestionnaireFlow/QuestionnaireFlow'
import { LanguageProvider } from '../../i18n/LanguageContext'
import { LanguageSwitcher } from '../../components/LanguageSwitcher/LanguageSwitcher'
import { useMessages } from '../../i18n/useMessages'
import { joinInvite, validateInvite } from '../../services/comparisonApi'
import type { AnswerSubmission, JoinInviteResult } from '../../types/api'
import styles from './InvitePage.module.css'

type Stage = 'validating' | 'invalid' | 'consent' | 'questionnaire' | 'joined' | 'declined'

/** The `/invite` route: an invitee opens their single-use invite link, whose token rides in the
 *  URL fragment (`/invite#TOKEN`) — same privacy property as `/me#TOKEN`. The flow (T026): validate
 *  the token without consuming it → §V consent (with the invitee's self-label) → the same
 *  questionnaire → join, which returns the invitee's own private link + access code. Declining
 *  creates nothing. */
export function InvitePage() {
  return (
    <LanguageProvider>
      <InvitePageView />
    </LanguageProvider>
  )
}

function InvitePageView() {
  const m = useMessages()
  const [stage, setStage] = useState<Stage>('validating')
  const [token, setToken] = useState('')
  const [inviterLabel, setInviterLabel] = useState('')
  const [inviteeLabel, setInviteeLabel] = useState('')
  const [credentials, setCredentials] = useState<JoinInviteResult | null>(null)
  const [joining, setJoining] = useState(false)
  const [joinError, setJoinError] = useState<string | null>(null)

  // Validate the fragment token once on mount, without consuming it, so the consent screen can
  // show the inviter's label. Any failure (unknown / used / expired) shows the neutral invalid state.
  useEffect(() => {
    const fragmentToken = window.location.hash.replace(/^#/, '')
    if (!fragmentToken) {
      setStage('invalid')
      return
    }
    setToken(fragmentToken)

    let cancelled = false
    validateInvite(fragmentToken)
      .then(invite => {
        if (cancelled) return
        setInviterLabel(invite.inviterLabel)
        setStage('consent')
      })
      .catch(() => {
        if (!cancelled) setStage('invalid')
      })
    return () => { cancelled = true }
  }, [])

  function handleAccept(label: string) {
    setInviteeLabel(label)
    setStage('questionnaire')
  }

  async function handleComplete(answers: AnswerSubmission[]) {
    if (joining) return
    setJoining(true)
    setJoinError(null)
    try {
      const result = await joinInvite({ token, consent: true, inviteeLabel, answers })
      setCredentials(result)
      setStage('joined')
      // The invite is consumed; scrub the token from history like /me does.
      window.history.replaceState(null, '', window.location.pathname)
    } catch {
      setJoinError(m.invite.joinError)
    } finally {
      setJoining(false)
    }
  }

  if (stage === 'consent') {
    return (
      <InviteeConsentStep
        inviterLabel={inviterLabel}
        onAccept={handleAccept}
        onDecline={() => setStage('declined')}
      />
    )
  }

  if (stage === 'questionnaire') {
    return (
      <QuestionnaireFlow
        onComplete={handleComplete}
        onExit={() => setStage('consent')}
        submitting={joining}
        submitError={joinError}
      />
    )
  }

  if (stage === 'joined' && credentials) {
    return (
      <div className={styles.center}>
        <span className={styles.brand}>common ground</span>
        <h1 className={styles.title}>{m.invite.joinedTitle}</h1>
        <p className={styles.message}>{m.invite.joinedIntro}</p>
        <CredentialsDisplay
          privateResultLink={credentials.privateResultLink}
          accessCode={credentials.accessCode}
        />
      </div>
    )
  }

  if (stage === 'declined') {
    return (
      <div className={styles.center}>
        <span className={styles.brand}>common ground</span>
        <h1 className={styles.title}>{m.invite.declinedTitle}</h1>
        <p className={styles.message}>{m.invite.declinedBody}</p>
      </div>
    )
  }

  // 'validating' and 'invalid' share the simple centered frame.
  return (
    <div className={styles.center}>
      <span className={styles.brand}>common ground</span>
      <LanguageSwitcher />

      {stage === 'validating' ? (
        <p className={styles.message}>{m.invite.loading}</p>
      ) : (
        <>
          <h1 className={styles.title}>{m.invite.invalidTitle}</h1>
          <p className={styles.message}>{m.invite.invalidBody}</p>
        </>
      )}
    </div>
  )
}
