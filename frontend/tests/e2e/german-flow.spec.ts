import { test, expect } from '@playwright/test'
import { mockApi } from './fixtures'

// T041 — full German completion flow (consent → questions → reflection), proving
// the language switch and the locale-aware fetch/submit work end to end in a real
// browser (SC-001, SC-002). API is mocked so the test is hermetic and fast.
test.describe('German completion flow (US1)', () => {
  test('switch to German and complete the questionnaire end to end', async ({ page }) => {
    const api = await mockApi(page)
    await page.goto('/')

    // Defaults to English (no browser auto-detect).
    await expect(page.locator('html')).toHaveAttribute('lang', 'en')
    await expect(page.getByRole('heading', { name: 'Find your common ground.' })).toBeVisible()

    // Switching language updates <html lang> and the chrome.
    await page.getByRole('button', { name: 'Deutsch' }).click()
    await expect(page.locator('html')).toHaveAttribute('lang', 'de')
    await expect(page.getByRole('heading', { name: 'Findet heraus, wo ihr ähnlich tickt.' })).toBeVisible()

    // Welcome → consent.
    await page.getByRole('button', { name: "Los geht's" }).click()
    await expect(page.getByRole('heading', { name: 'Bevor wir beginnen' })).toBeVisible()
    await page.getByRole('checkbox').check()
    await page.getByRole('button', { name: 'Beginnen' }).click()

    // Question 1 (German content), pick an answer, advance.
    await expect(page.getByText('Frage 1 / 2')).toBeVisible()
    await expect(page.getByRole('heading', { name: /Wenn du mit jemandem neu/ })).toBeVisible()
    await page.getByRole('button', { name: /Schriftliche Ziele/ }).click()
    await page.getByRole('button', { name: 'Weiter' }).click()

    // Question 2 (last) → submit.
    await expect(page.getByText('Frage 2 / 2')).toBeVisible()
    await page.getByRole('button', { name: /Eine schriftliche Zusammenfassung/ }).click()
    await page.getByRole('button', { name: 'Absenden' }).click()

    // Completion (German), open the reflection.
    await expect(page.getByRole('heading', { name: 'Deine Reflexion ist fertig.' })).toBeVisible()
    await page.getByRole('button', { name: 'Meine Reflexion ansehen' }).click()

    // Reflection renders in German: locale-driven chrome heading + fixture content.
    await expect(page.getByRole('heading', { name: 'Wie du arbeitest' })).toBeVisible()
    await expect(page.getByText('Feste, planbare Arbeitszyklen')).toBeVisible()

    // The answers were submitted with the German locale.
    expect(api.submittedLocales).toContain('de')
  })
})
