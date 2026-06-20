import type { Page, Route } from '@playwright/test'

// Locale-aware API fixtures so the E2E flow runs end to end without a backend.
// Each handler reads `?locale=` and returns the matching language, mirroring the
// real API contract (SupportedLocales: en default, de). Option and question IDs
// are shared across locales — that locale-invariance is what lets a mid-flow
// language switch preserve the in-progress answer.

type Locale = 'en' | 'de'

interface AnswerOption { id: string; text: string; orderIndex: number }
interface Question { id: string; text: string; sectionIndex: number; orderIndex: number; answerOptions: AnswerOption[] }
interface Questionnaire { id: string; versionNumber: string; questions: Question[] }
interface Insight { dimensionId: string; title: string; text: string; strength: number }
interface ReflectionGroup { id: string; title: string; insights: Insight[] }
interface Reflection { groups: ReflectionGroup[] }

// Stable IDs, reused for both locales.
const Q1 = 'question-collaboration-start'
const Q2 = 'question-shared-foundation'
const Q1_OPTS = ['q1-written', 'q1-conversation', 'q1-examples', 'q1-ownership']
const Q2_OPTS = ['q2-summary', 'q2-board', 'q2-ownership']

const questionnaire: Record<Locale, Questionnaire> = {
  en: {
    id: 'questionnaire-current',
    versionNumber: '1.0.0',
    questions: [
      {
        id: Q1, sectionIndex: 1, orderIndex: 1,
        text: 'When you start working with someone new, what helps you get into a good working rhythm?',
        answerOptions: [
          { id: Q1_OPTS[0], orderIndex: 1, text: 'Written goals, context, and expectations' },
          { id: Q1_OPTS[1], orderIndex: 2, text: 'A direct conversation to build shared understanding' },
          { id: Q1_OPTS[2], orderIndex: 3, text: 'Seeing examples or trying a small piece of real work' },
          { id: Q1_OPTS[3], orderIndex: 4, text: 'Clear ownership and decision boundaries' },
        ],
      },
      {
        id: Q2, sectionIndex: 1, orderIndex: 2,
        text: 'After the first alignment: what shared foundation helps you keep working well?',
        answerOptions: [
          { id: Q2_OPTS[0], orderIndex: 1, text: 'A written summary of decisions and next steps' },
          { id: Q2_OPTS[1], orderIndex: 2, text: 'A shared board or plan with priorities' },
          { id: Q2_OPTS[2], orderIndex: 3, text: 'Clear task ownership in the work system' },
        ],
      },
    ],
  },
  de: {
    id: 'questionnaire-current',
    versionNumber: '1.0.0',
    questions: [
      {
        id: Q1, sectionIndex: 1, orderIndex: 1,
        text: 'Wenn du mit jemandem neu zusammenarbeitest, was hilft dir, gut ins gemeinsame Arbeiten zu kommen?',
        answerOptions: [
          { id: Q1_OPTS[0], orderIndex: 1, text: 'Schriftliche Ziele, Kontext und Erwartungen' },
          { id: Q1_OPTS[1], orderIndex: 2, text: 'Ein direktes Gespräch, um ein gemeinsames Verständnis zu entwickeln' },
          { id: Q1_OPTS[2], orderIndex: 3, text: 'Beispiele sehen oder ein kleines Stück echte Arbeit ausprobieren' },
          { id: Q1_OPTS[3], orderIndex: 4, text: 'Klare Verantwortung und Entscheidungsgrenzen' },
        ],
      },
      {
        id: Q2, sectionIndex: 1, orderIndex: 2,
        text: 'Nach der ersten Abstimmung: Welche gemeinsame Grundlage hilft dir, gut weiterzuarbeiten?',
        answerOptions: [
          { id: Q2_OPTS[0], orderIndex: 1, text: 'Eine schriftliche Zusammenfassung von Entscheidungen und nächsten Schritten' },
          { id: Q2_OPTS[1], orderIndex: 2, text: 'Ein gemeinsames Board oder ein Plan mit Prioritäten' },
          { id: Q2_OPTS[2], orderIndex: 3, text: 'Klare Aufgabenverantwortung im Arbeitssystem' },
        ],
      },
    ],
  },
}

const reflection: Record<Locale, Reflection> = {
  en: {
    groups: [
      {
        id: 'how_you_plan_and_handle_change',
        title: 'How you plan and handle change',
        insights: [
          {
            dimensionId: 'iteration_preference',
            title: 'Fixed, predictable planning cycles',
            strength: 5,
            text: 'Fixed cycles work well for you — a predictable cadence with clear moments to plan, deliver, and reflect.',
          },
        ],
      },
    ],
  },
  de: {
    groups: [
      {
        id: 'how_you_plan_and_handle_change',
        title: 'Wie du planst und mit Veränderung umgehst',
        insights: [
          {
            dimensionId: 'iteration_preference',
            title: 'Feste, planbare Arbeitszyklen',
            strength: 5,
            text: 'Feste Zyklen passen gut zu dir — ein planbarer Rhythmus mit klaren Momenten zum Planen, Liefern und Reflektieren.',
          },
        ],
      },
    ],
  },
}

function localeFromUrl(url: string): Locale {
  return new URL(url).searchParams.get('locale') === 'de' ? 'de' : 'en'
}

/** Records the locales the frontend requested, so a test can assert that switching
 *  language actually drove a locale-aware fetch (not just a client-side re-render). */
export interface ApiCalls {
  submittedLocales: string[]
  meReflectionLocales: string[]
}

/** Intercepts every `/api/**` call and serves locale-aware fixtures. Returns a record
 *  of the locales requested for the submit and `/me` endpoints. */
export async function mockApi(page: Page): Promise<ApiCalls> {
  const calls: ApiCalls = { submittedLocales: [], meReflectionLocales: [] }

  await page.route('**/api/questionnaire/current**', (route: Route) =>
    route.fulfill({ json: questionnaire[localeFromUrl(route.request().url())] }),
  )

  await page.route('**/api/responses**', (route: Route) => {
    const locale = localeFromUrl(route.request().url())
    calls.submittedLocales.push(locale)
    return route.fulfill({
      status: 201,
      json: {
        privateResultLink: 'https://common-ground.test/me#FIXTURE-TOKEN',
        accessCode: 'FIXTURE-CODE',
        reflection: reflection[locale],
      },
    })
  })

  await page.route('**/api/session/start**', (route: Route) =>
    route.fulfill({ status: 200, body: '' }),
  )

  await page.route('**/api/me/reflection**', (route: Route) => {
    const locale = localeFromUrl(route.request().url())
    calls.meReflectionLocales.push(locale)
    return route.fulfill({ json: { reflection: reflection[locale], accessCodeAvailable: true } })
  })

  return calls
}
