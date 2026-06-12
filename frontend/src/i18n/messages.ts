/**
 * Shape of a UI-chrome message catalog. English and German catalogs both satisfy this
 * interface, so TypeScript strict mode enforces that every key exists in every language.
 * Grows as components are localized (T025); keep both catalogs in lock-step.
 */
export interface Messages {
  /** Language switcher. Language names are endonyms (shown the same in every locale). */
  language: {
    label: string
    en: string
    de: string
  }
}
