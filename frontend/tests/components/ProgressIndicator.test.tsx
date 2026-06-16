import { screen } from '@testing-library/react'
import { renderWithLocale } from '../support/renderWithLocale'
import { describe, expect, it } from 'vitest'
import { ProgressIndicator } from '../../src/components/ProgressIndicator/ProgressIndicator'

describe('ProgressIndicator', () => {
  it('shows the current question out of the total', () => {
    renderWithLocale(
      <ProgressIndicator current={3} total={46} sectionCurrent={1} sectionTotal={10} />,
    )

    expect(screen.getByLabelText('Question 3 of 46')).toBeInTheDocument()
  })

  it('shows the current section out of the total', () => {
    renderWithLocale(
      <ProgressIndicator current={3} total={46} sectionCurrent={2} sectionTotal={10} />,
    )

    expect(screen.getByLabelText('Section 2 of 10')).toBeInTheDocument()
    expect(screen.getByText('Section 2 of 10')).toBeInTheDocument()
  })
})
