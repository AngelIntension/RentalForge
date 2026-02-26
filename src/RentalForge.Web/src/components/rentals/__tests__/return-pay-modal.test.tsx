import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ReturnPayModal } from '@/components/rentals/return-pay-modal'

describe('ReturnPayModal', () => {
  const defaultProps = {
    open: true,
    onClose: vi.fn(),
    onSubmit: vi.fn(),
    rentalRate: 4.99,
    isLoading: false,
  }

  it('renders with amount pre-filled from rentalRate', () => {
    render(<ReturnPayModal {...defaultProps} />)

    const amountInput = screen.getByLabelText(/amount/i) as HTMLInputElement
    expect(amountInput.value).toBe('4.99')
  })

  it('renders dialog title', () => {
    render(<ReturnPayModal {...defaultProps} />)

    expect(screen.getByRole('heading', { name: 'Return & Pay' })).toBeInTheDocument()
  })

  it('shows validation error for zero amount', async () => {
    render(<ReturnPayModal {...defaultProps} />)

    const user = userEvent.setup()
    const amountInput = screen.getByLabelText(/amount/i)
    await user.clear(amountInput)
    await user.type(amountInput, '0')
    await user.type(screen.getByLabelText(/staff id/i), '1')
    await user.click(screen.getByRole('button', { name: /return & pay/i }))

    expect(screen.getByText(/amount must be greater than 0/i)).toBeInTheDocument()
    expect(defaultProps.onSubmit).not.toHaveBeenCalled()
  })

  it('shows validation error for missing staffId', async () => {
    render(<ReturnPayModal {...defaultProps} />)

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /return & pay/i }))

    expect(screen.getByText(/staff id is required/i)).toBeInTheDocument()
    expect(defaultProps.onSubmit).not.toHaveBeenCalled()
  })

  it('calls onSubmit with correct data', async () => {
    const onSubmit = vi.fn()
    render(<ReturnPayModal {...defaultProps} onSubmit={onSubmit} />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/staff id/i), '1')
    await user.click(screen.getByRole('button', { name: /return & pay/i }))

    expect(onSubmit).toHaveBeenCalledWith({ amount: 4.99, staffId: 1 })
  })

  it('calls onClose when Cancel clicked', async () => {
    const onClose = vi.fn()
    render(<ReturnPayModal {...defaultProps} onClose={onClose} />)

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /cancel/i }))

    expect(onClose).toHaveBeenCalled()
  })

  it('disables submit button when loading', () => {
    render(<ReturnPayModal {...defaultProps} isLoading={true} />)

    expect(screen.getByRole('button', { name: /processing/i })).toBeDisabled()
  })
})
