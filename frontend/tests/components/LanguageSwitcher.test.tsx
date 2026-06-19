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

  it('is operable by keyboard — tab to a locale and activate with Enter', async () => {
    const user = userEvent.setup()
    renderSwitcher()

    await user.tab()
    expect(screen.getByRole('button', { name: 'English' })).toHaveFocus()
    await user.tab()
    expect(screen.getByRole('button', { name: 'Deutsch' })).toHaveFocus()

    await user.keyboard('{Enter}')
    expect(screen.getByRole('button', { name: 'Deutsch' })).toHaveAttribute('aria-pressed', 'true')
    expect(document.documentElement.lang).toBe('de')
  })

  it('exposes a polite live region for announcing the change to assistive tech', () => {
    renderSwitcher()
    // The region is present (and empty) from first render; the announcement text is
    // asserted in the mid-flow switch test where an actual change occurs.
    expect(screen.getByRole('status')).toHaveAttribute('aria-live', 'polite')
  })
})
