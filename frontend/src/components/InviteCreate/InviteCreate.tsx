import { useState } from 'react'
import type { FormEvent } from 'react'
import { createInvite } from '../../services/comparisonApi'
import { useMessages } from '../../i18n/useMessages'
import styles from './InviteCreate.module.css'

interface Props {
  /** Called when the user dismisses the panel (cancel, or done after the link is shown). */
  onClose: () => void
}

type Status = 'idle' | 'creating' | 'created' | 'error'

// Matches the backend Invite.InviterLabel / ComparisonParticipant.DisplayLabel column length.
const MAX_LABEL_LENGTH = 60

/** US1 (T017) — the inviter labels themselves and mints a single-use, time-limited invite
 *  from their own `/me` reflection, then copies the shareable `/invite#<token>` link. */
export function InviteCreate({ onClose }: Readonly<Props>) {
  const m = useMessages()
  const [label, setLabel] = useState('')
  const [status, setStatus] = useState<Status>('idle')
  const [link, setLink] = useState('')
  const [copied, setCopied] = useState(false)

  async function submit(e: FormEvent) {
    e.preventDefault()
    const trimmed = label.trim()
    if (trimmed.length === 0 || status === 'creating') return
    setStatus('creating')
    try {
      const result = await createInvite(trimmed)
      const origin = 'window' in globalThis ? globalThis.location.origin : ''
      setLink(`${origin}/invite#${result.inviteToken}`)
      setStatus('created')
    } catch {
      setStatus('error')
    }
  }

  async function copy() {
    try {
      await navigator.clipboard.writeText(link)
      setCopied(true)
      setTimeout(() => setCopied(false), 1600)
    } catch {
      // Clipboard may be unavailable (e.g. insecure context); fail quietly.
    }
  }

  return (
    <div className={styles.backdrop} role="dialog" aria-modal="true" aria-labelledby="invite-create-heading">
      <div className={styles.panel}>
        {status === 'created' ? (
          <>
            <h2 id="invite-create-heading" className={styles.heading}>{m.inviteCreate.linkTitle}</h2>
            <p className={styles.intro}>{m.inviteCreate.linkNote}</p>
            <div className={styles.linkRow}>
              <code className={styles.link}>{link}</code>
              <button type="button" className={styles.copyBtn} onClick={copy} aria-label={m.inviteCreate.copyAria}>
                {copied ? m.inviteCreate.copied : m.inviteCreate.copy}
              </button>
            </div>
            <button type="button" className={styles.secondary} onClick={onClose}>
              {m.shell.back}
            </button>
          </>
        ) : (
          <form className={styles.form} onSubmit={submit}>
            <h2 id="invite-create-heading" className={styles.heading}>{m.inviteCreate.heading}</h2>
            <p className={styles.intro}>{m.inviteCreate.intro}</p>

            <label className={styles.label} htmlFor="invite-label">{m.inviteCreate.labelLabel}</label>
            <input
              id="invite-label"
              className={styles.input}
              type="text"
              value={label}
              maxLength={MAX_LABEL_LENGTH}
              placeholder={m.inviteCreate.labelPlaceholder}
              onChange={e => setLabel(e.target.value)}
            />
            <p className={styles.hint}>{m.inviteCreate.labelHint}</p>

            {status === 'error' && <p className={styles.error} role="alert">{m.inviteCreate.error}</p>}

            <div className={styles.actions}>
              <button type="button" className={styles.secondary} onClick={onClose}>
                {m.inviteCreate.cancel}
              </button>
              <button type="submit" className={styles.primary} disabled={label.trim().length === 0 || status === 'creating'}>
                {status === 'creating' ? m.inviteCreate.creating : m.inviteCreate.create}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  )
}
