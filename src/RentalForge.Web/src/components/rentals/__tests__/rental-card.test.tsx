import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { RentalCard } from '@/components/rentals/rental-card'
import { sampleRentalListItem, sampleRentalListItems } from '@/test/fixtures/data'

const mockUseAuth = vi.fn()

vi.mock('@/hooks/use-auth', () => ({
  useAuth: () => mockUseAuth(),
}))

function renderWithRouter(ui: React.ReactElement) {
  return render(<MemoryRouter>{ui}</MemoryRouter>)
}

describe('RentalCard', () => {
  beforeEach(() => {
    mockUseAuth.mockReset()
  })

  it('displays rental info with payment status', () => {
    mockUseAuth.mockReturnValue({ role: 'Customer' })
    renderWithRouter(<RentalCard rental={sampleRentalListItem} />)

    expect(screen.getByText(`Rental #${sampleRentalListItem.id}`)).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    // Verify Paid and Balance labels are rendered
    expect(screen.getByText(/Paid:/)).toBeInTheDocument()
    expect(screen.getByText(/Balance:/)).toBeInTheDocument()
  })

  it('shows Returned badge for returned rental', () => {
    mockUseAuth.mockReturnValue({ role: 'Customer' })
    const returnedRental = sampleRentalListItems[1]
    renderWithRouter(<RentalCard rental={returnedRental} />)

    expect(screen.getByText('Returned')).toBeInTheDocument()
  })

  it('shows Return button for Customer on active rental', () => {
    mockUseAuth.mockReturnValue({ role: 'Customer' })
    const onReturn = vi.fn()
    renderWithRouter(<RentalCard rental={sampleRentalListItem} onReturn={onReturn} />)

    expect(screen.getByRole('button', { name: /^return$/i })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /return & pay/i })).not.toBeInTheDocument()
  })

  it('shows Return & Pay and Return Only buttons for Staff on active rental', () => {
    mockUseAuth.mockReturnValue({ role: 'Staff' })
    const onReturn = vi.fn()
    const onReturnAndPay = vi.fn()
    renderWithRouter(
      <RentalCard rental={sampleRentalListItem} onReturn={onReturn} onReturnAndPay={onReturnAndPay} />,
    )

    expect(screen.getByRole('button', { name: /return & pay/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /return only/i })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /^return$/i })).not.toBeInTheDocument()
  })

  it('shows Return & Pay button for Admin on active rental', () => {
    mockUseAuth.mockReturnValue({ role: 'Admin' })
    const onReturnAndPay = vi.fn()
    renderWithRouter(
      <RentalCard rental={sampleRentalListItem} onReturnAndPay={onReturnAndPay} />,
    )

    expect(screen.getByRole('button', { name: /return & pay/i })).toBeInTheDocument()
  })

  it('hides action buttons for returned rental', () => {
    mockUseAuth.mockReturnValue({ role: 'Staff' })
    const returnedRental = sampleRentalListItems[1]
    const onReturn = vi.fn()
    const onReturnAndPay = vi.fn()
    renderWithRouter(
      <RentalCard rental={returnedRental} onReturn={onReturn} onReturnAndPay={onReturnAndPay} />,
    )

    expect(screen.queryByRole('button', { name: /return/i })).not.toBeInTheDocument()
  })

  it('calls onReturnAndPay when Return & Pay clicked', async () => {
    mockUseAuth.mockReturnValue({ role: 'Staff' })
    const onReturnAndPay = vi.fn()
    renderWithRouter(
      <RentalCard rental={sampleRentalListItem} onReturnAndPay={onReturnAndPay} />,
    )

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /return & pay/i }))

    expect(onReturnAndPay).toHaveBeenCalledWith(sampleRentalListItem.id)
  })

  it('calls onReturn when Return Only clicked', async () => {
    mockUseAuth.mockReturnValue({ role: 'Staff' })
    const onReturn = vi.fn()
    renderWithRouter(
      <RentalCard rental={sampleRentalListItem} onReturn={onReturn} onReturnAndPay={vi.fn()} />,
    )

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /return only/i }))

    expect(onReturn).toHaveBeenCalledWith(sampleRentalListItem.id)
  })

  it('color-codes outstanding balance green when paid', () => {
    mockUseAuth.mockReturnValue({ role: 'Customer' })
    const paidRental = sampleRentalListItems[1] // outstandingBalance: 0
    renderWithRouter(<RentalCard rental={paidRental} />)

    const balanceParagraph = screen.getByText(/Balance:/).closest('p')!
    const balanceSpan = within(balanceParagraph).getByText(`$${paidRental.outstandingBalance.toFixed(2)}`)
    expect(balanceSpan.className).toContain('text-green-600')
  })

  it('color-codes outstanding balance amber when unpaid', () => {
    mockUseAuth.mockReturnValue({ role: 'Customer' })
    renderWithRouter(<RentalCard rental={sampleRentalListItem} />) // outstandingBalance: 4.99

    const balanceParagraph = screen.getByText(/Balance:/).closest('p')!
    const balanceSpan = within(balanceParagraph).getByText(`$${sampleRentalListItem.outstandingBalance.toFixed(2)}`)
    expect(balanceSpan.className).toContain('text-amber-600')
  })
})
