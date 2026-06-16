import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { LanguageProvider } from '../../src/i18n/LanguageContext'
import { LOCALE_STORAGE_KEY, type Locale } from '../../src/i18n/locales'

/**
 * Renders a component inside the LanguageProvider so anything using `useMessages` works.
 * The locale is written to localStorage before mount (the provider reads it there), so
 * `renderWithLocale(<X/>, 'de')` exercises the German chrome. Using the RTL `wrapper`
 * option means `rerender` re-applies the provider too.
 */
export function renderWithLocale(ui: ReactElement, locale: Locale = 'en') {
  localStorage.setItem(LOCALE_STORAGE_KEY, locale)
  return render(ui, { wrapper: LanguageProvider })
}
