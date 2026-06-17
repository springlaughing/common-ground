import { useEffect, useRef, useState } from 'react'
import { useLanguage } from '../../i18n/LanguageContext'
import { useMessages } from '../../i18n/useMessages'
import { SUPPORTED_LOCALES, type Locale } from '../../i18n/locales'
import styles from './LanguageSwitcher.module.css'

/** Accessible EN/DE toggle. Shows compact language codes; exposes the full (localized)
 *  language name to assistive tech and marks the active locale with aria-pressed. A
 *  switch is otherwise silent for screen-reader users (only `<html lang>` and the
 *  pressed state change), so it's announced via a polite live region. */
export function LanguageSwitcher() {
  const { locale, setLocale } = useLanguage()
  const m = useMessages()
  const names: Record<Locale, string> = { en: m.language.en, de: m.language.de }

  // Announce locale changes, but stay silent on first render (and StrictMode's remount):
  // comparing against the previous value means we only speak on an actual change.
  const [announcement, setAnnouncement] = useState('')
  const prevLocale = useRef(locale)
  useEffect(() => {
    if (prevLocale.current !== locale) {
      prevLocale.current = locale
      setAnnouncement(m.language.changed(m.language[locale]))
    }
  }, [locale, m])

  return (
    <div className={styles.switcher} role="group" aria-label={m.language.label}>
      {SUPPORTED_LOCALES.map(code => (
        <button
          key={code}
          type="button"
          className={styles.option}
          aria-pressed={code === locale}
          aria-label={names[code]}
          onClick={() => setLocale(code)}
        >
          {code.toUpperCase()}
        </button>
      ))}
      <span role="status" aria-live="polite" className={styles.srOnly}>
        {announcement}
      </span>
    </div>
  )
}
