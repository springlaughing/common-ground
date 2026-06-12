import { useLanguage } from './LanguageContext'
import type { Messages } from './messages'
import { en } from './messages.en'
import { de } from './messages.de'
import type { Locale } from './locales'

const catalogs: Record<Locale, Messages> = { en, de }

/** Returns the UI-chrome message catalog for the active locale. */
export function useMessages(): Messages {
  const { locale } = useLanguage()
  return catalogs[locale]
}
