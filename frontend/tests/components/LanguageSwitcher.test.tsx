import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'
import { LanguageProvider } from '../../src/i18n/LanguageContext'
import { LanguageSwitcher } from '../../src/components/LanguageSwitcher/LanguageSwitcher'

function renderSwitcher() {
  return render(
    <LanguageProvider>
      <LanguageSwitcher />
    </LanguageProvider>,
  )
}

describe('LanguageSwitcher', () => {
  beforeEach(() => localStorage.clear())

  it('renders both locale options, with English active by default', () => {
    renderSwitcher()
    expect(screen.getByRole('button', { name: 'English' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('button', { name: 'Deutsch' })).toHaveAttribute('aria-pressed', 'false')
  })

  it('exposes an accessible group label', () => {
    renderSwitcher()
    expect(screen.getByRole('group', { name: 'Language' })).toBeInTheDocument()
  })

  it('switches the active locale on click and updates <html lang>', async () => {
    const user = userEvent.setup()
    renderSwitcher()

    await user.click(screen.getByRole('button', { name: 'Deutsch' }))

    expect(screen.getByRole('button', { name: 'Deutsch' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('button', { name: 'English' })).toHaveAttribute('aria-pressed', 'false')
    expect(document.documentElement.lang).toBe('de')
  })
})
