import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react'
import { DEFAULT_LOCALE, isLocale, LOCALE_STORAGE_KEY, type Locale } from './locales'

interface LanguageContextValue {
  locale: Locale
  setLocale: (locale: Locale) => void
}

const LanguageContext = createContext<LanguageContextValue | undefined>(undefined)

function readStoredLocale(): Locale {
  try {
    const stored = globalThis.localStorage?.getItem(LOCALE_STORAGE_KEY)
    return isLocale(stored) ? stored : DEFAULT_LOCALE
  } catch {
    return DEFAULT_LOCALE
  }
}

/** Holds the active locale (accountless, persisted to localStorage) and keeps the
 *  document's `lang` attribute in sync for assistive technology. */
export function LanguageProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [locale, setLocaleState] = useState<Locale>(readStoredLocale)

  useEffect(() => {
    document.documentElement.lang = locale
  }, [locale])

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next)
    try {
      globalThis.localStorage?.setItem(LOCALE_STORAGE_KEY, next)
    } catch {
      // Ignore storage failures (e.g. private mode) — the in-memory locale still applies.
    }
  }, [])

  return (
    <LanguageContext.Provider value={{ locale, setLocale }}>
      {children}
    </LanguageContext.Provider>
  )
}

export function useLanguage(): LanguageContextValue {
  const ctx = useContext(LanguageContext)
  if (!ctx) throw new Error('useLanguage must be used within a LanguageProvider')
  return ctx
}
