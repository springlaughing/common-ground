import '@testing-library/jest-dom'
import { vi } from 'vitest'

// jsdom doesn't implement scrollTo; App scrolls to top on question/stage change.
window.scrollTo = vi.fn()
