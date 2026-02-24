import { describe, it, expect, vi } from 'vitest'
import { render, screen, userEvent } from '@/test/test-utils'
import { RentalForm } from '@/components/rentals/rental-form'

describe('RentalForm', () => {
  it('shows required field errors when empty', async () => {
    const onSubmit = vi.fn()
    render(<RentalForm onSubmit={onSubmit} />)

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /create rental/i }))

    // All 4 fields should show errors
    expect(screen.getAllByText(/is required|must be/i).length).toBeGreaterThanOrEqual(1)
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('calls onSubmit with valid data', async () => {
    const onSubmit = vi.fn()
    render(<RentalForm onSubmit={onSubmit} />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/film id/i), '80')
    await user.type(screen.getByLabelText(/store id/i), '1')
    await user.type(screen.getByLabelText(/customer id/i), '130')
    await user.type(screen.getByLabelText(/staff id/i), '1')
    await user.click(screen.getByRole('button', { name: /create rental/i }))

    expect(onSubmit).toHaveBeenCalledWith({
      filmId: 80,
      storeId: 1,
      customerId: 130,
      staffId: 1,
    })
  })

  it('displays API validation errors', () => {
    const errors = { filmId: ['No available inventory for this film at this store'] }
    render(<RentalForm onSubmit={() => {}} apiErrors={errors} />)

    expect(screen.getByText(/no available inventory/i)).toBeInTheDocument()
  })
})
