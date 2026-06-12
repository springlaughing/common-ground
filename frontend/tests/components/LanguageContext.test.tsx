import { render, screen, act } from '@testing-library/react'
import { beforeEach, describe, expect, it } from 'vitest'
import { LanguageProvider, useLanguage } from '../../src/i18n/LanguageContext'
import { LOCALE_STORAGE_KEY } from '../../src/i18n/locales'

function Probe() {
  const { locale, setLocale } = useLanguage()
  return (
    <div>
      <span data-testid="locale">{locale}</span>
      <button onClick={() => setLocale('de')}>to-de</button>
    </div>
  )
}

function renderProbe() {
  return render(
    <LanguageProvider>
      <Probe />
    </LanguageProvider>,
  )
}

describe('LanguageContext', () => {
  beforeEach(() => localStorage.clear())

  it('defaults to English and sets <html lang> when nothing is stored', () => {
    renderProbe()
    expect(screen.getByTestId('locale')).toHaveTextContent('en')
    expect(document.documentElement.lang).toBe('en')
  })

  it('persists the chosen locale and updates <html lang>', () => {
    renderProbe()
    act(() => screen.getByText('to-de').click())

    expect(screen.getByTestId('locale')).toHaveTextContent('de')
    expect(localStorage.getItem(LOCALE_STORAGE_KEY)).toBe('de')
    expect(document.documentElement.lang).toBe('de')
  })

  it('reads a previously stored locale on mount', () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'de')
    renderProbe()
    expect(screen.getByTestId('locale')).toHaveTextContent('de')
  })

  it('ignores an invalid stored value and falls back to English', () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'fr')
    renderProbe()
    expect(screen.getByTestId('locale')).toHaveTextContent('en')
  })
})
