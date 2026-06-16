import { useLanguage } from '../../i18n/LanguageContext'
import { useMessages } from '../../i18n/useMessages'
import { SUPPORTED_LOCALES, type Locale } from '../../i18n/locales'
import styles from './LanguageSwitcher.module.css'

/** Accessible EN/DE toggle. Shows compact language codes; exposes the full (localized)
 *  language name to assistive tech and marks the active locale with aria-pressed. */
export function LanguageSwitcher() {
  const { locale, setLocale } = useLanguage()
  const m = useMessages()
  const names: Record<Locale, string> = { en: m.language.en, de: m.language.de }

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
    </div>
  )
}
