import { test, expect } from '@playwright/test'
import { mockApi } from './fixtures'

// T042 — language switching preserves state: mid-questionnaire (answers + position)
// and on the saved `/me` reflection (re-fetch in the viewer's language). SC-003, SC-006.

test.describe('Mid-flow language switch (US2)', () => {
  test('switching language keeps the current question and the chosen answer', async ({ page }) => {
    await mockApi(page)
    await page.goto('/')

    // English: walk to question 1 and choose a primary answer.
    await page.getByRole('button', { name: 'Get started' }).click()
    await page.getByRole('checkbox').check()
    await page.getByRole('button', { name: 'Begin' }).click()
    await expect(page.getByText('Question 1 / 2')).toBeVisible()

    const englishChoice = page.getByRole('button', { name: /Written goals, context/ })
    await englishChoice.click()
    await expect(englishChoice).toHaveAttribute('aria-pressed', 'true')

    // Switch to German mid-question.
    await page.getByRole('button', { name: 'Deutsch' }).click()

    // Still on question 1, now in German…
    await expect(page.getByText('Frage 1 / 2')).toBeVisible()
    await expect(page.getByRole('heading', { name: /Wenn du mit jemandem neu/ })).toBeVisible()

    // …and the same option (keyed by its locale-invariant ID) is still selected.
    const germanChoice = page.getByRole('button', { name: /Schriftliche Ziele/ })
    await expect(germanChoice).toHaveAttribute('aria-pressed', 'true')
  })
})

test.describe('Saved reflection language switch (US4)', () => {
  test('switching language on /me re-fetches and re-renders the reflection', async ({ page }) => {
    const api = await mockApi(page)
    await page.goto('/me#FIXTURE-TOKEN')

    // Loads in English (default).
    await expect(page.getByRole('heading', { name: 'How you work' })).toBeVisible()
    await expect(page.getByText('Fixed, predictable planning cycles')).toBeVisible()

    // Switch to German → re-fetch and re-render in place.
    await page.getByRole('button', { name: 'Deutsch' }).click()
    await expect(page.getByRole('heading', { name: 'Wie du arbeitest' })).toBeVisible()
    await expect(page.getByText('Feste, planbare Arbeitszyklen')).toBeVisible()

    // The reflection was re-requested in German (not just re-rendered client-side).
    expect(api.meReflectionLocales).toContain('de')
  })
})
