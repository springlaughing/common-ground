/** Supported UI locales. English is the default and fallback. Mirrors the backend
 *  `SupportedLocales` (en|de, default en). Adding a locale here + a message catalog
 *  + backend translation rows is all that a new language needs. */
export const SUPPORTED_LOCALES = ['en', 'de'] as const

export type Locale = (typeof SUPPORTED_LOCALES)[number]

export const DEFAULT_LOCALE: Locale = 'en'

/** localStorage key holding the user's chosen locale (accountless, per-device). */
export const LOCALE_STORAGE_KEY = 'cg_locale'

/** Narrows an arbitrary string to a supported {@link Locale}. */
export function isLocale(value: string | null | undefined): value is Locale {
  return value === 'en' || value === 'de'
}
