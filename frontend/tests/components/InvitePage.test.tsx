import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { InvitePage } from '../../src/pages/InvitePage/InvitePage'
import { LOCALE_STORAGE_KEY } from '../../src/i18n/locales'

// Phase 1 scaffold (T002): the placeholder /invite landing. The full validate→consent→
// questionnaire→credentials flow (and its tests) arrive in T026/T028; here we only assert
// the route renders the right state from the fragment token and localizes its chrome.
describe('InvitePage (/invite route)', () => {
  afterEach(() => {
    window.location.hash = ''
    localStorage.clear()
  })

  it('shows the invite landing when a fragment token is present', () => {
    window.location.hash = '#invite-token'
    render(<InvitePage />)

    expect(
      screen.getByText('You’ve been invited to compare working styles.'),
    ).toBeInTheDocument()
    expect(screen.queryByText('Invite not available')).not.toBeInTheDocument()
  })

  it('shows the invalid state when no token is in the fragment', () => {
    render(<InvitePage />)

    expect(screen.getByText('Invite not available')).toBeInTheDocument()
    expect(
      screen.queryByText('You’ve been invited to compare working styles.'),
    ).not.toBeInTheDocument()
  })

  it('renders the invite chrome in the active language (German)', () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'de')
    window.location.hash = '#invite-token'
    render(<InvitePage />)

    expect(
      screen.getByText('Du wurdest eingeladen, Arbeitsstile zu vergleichen.'),
    ).toBeInTheDocument()
  })
})
