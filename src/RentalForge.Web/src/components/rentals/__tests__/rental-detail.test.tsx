import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { RentalDetail } from '@/components/rentals/rental-detail'
import { sampleRentalDetail, sampleRentalPaymentItems } from '@/test/fixtures/data'
import type { RentalDetail as RentalDetailType } from '@/types/rental'

function renderWithRouter(ui: React.ReactElement) {
  return render(<MemoryRouter>{ui}</MemoryRouter>)
}

describe('RentalDetail', () => {
  it('displays rental information', () => {
    renderWithRouter(<RentalDetail rental={sampleRentalDetail} />)

    expect(screen.getByText(`Rental #${sampleRentalDetail.id}`)).toBeInTheDocument()
    expect(screen.getByText(sampleRentalDetail.filmTitle)).toBeInTheDocument()
    expect(screen.getByText(`${sampleRentalDetail.customerFirstName} ${sampleRentalDetail.customerLastName}`)).toBeInTheDocument()
  })

  it('displays payment summary fields', () => {
    renderWithRouter(<RentalDetail rental={sampleRentalDetail} />)

    expect(screen.getByText('Rental Rate')).toBeInTheDocument()
    expect(screen.getByText('Total Paid')).toBeInTheDocument()
    expect(screen.getByText('Outstanding Balance')).toBeInTheDocument()
  })

  it('shows payment history when rental has payments', () => {
    renderWithRouter(<RentalDetail rental={sampleRentalDetail} />)

    expect(screen.getByText('Payments')).toBeInTheDocument()
    // Payment amounts should be present (may appear in multiple places)
    for (const payment of sampleRentalPaymentItems) {
      expect(screen.getAllByText(`$${payment.amount.toFixed(2)}`).length).toBeGreaterThanOrEqual(1)
    }
    // Total row should be present
    expect(screen.getByText('Total')).toBeInTheDocument()
  })

  it('shows "No payments recorded" when payments array is empty', () => {
    const rentalWithNoPayments: RentalDetailType = {
      ...sampleRentalDetail,
      totalPaid: 0,
      outstandingBalance: sampleRentalDetail.rentalRate,
      payments: [],
    }
    renderWithRouter(<RentalDetail rental={rentalWithNoPayments} />)

    expect(screen.getByText('No payments recorded')).toBeInTheDocument()
    expect(screen.queryByText('Total')).not.toBeInTheDocument()
  })

  it('calculates total correctly from payments', () => {
    const multiplePayments = [
      { id: 5001, amount: 2.00, paymentDate: '2026-02-24T10:00:00Z', staffId: 1 },
      { id: 5002, amount: 1.50, paymentDate: '2026-02-25T14:00:00Z', staffId: 1 },
    ]
    const rental: RentalDetailType = {
      ...sampleRentalDetail,
      totalPaid: 3.50,
      payments: multiplePayments,
    }
    renderWithRouter(<RentalDetail rental={rental} />)

    // Both $2.00 and $1.50 should appear as individual payments
    expect(screen.getByText('$2.00')).toBeInTheDocument()
    expect(screen.getByText('$1.50')).toBeInTheDocument()
    // $3.50 appears in both Total Paid summary and payment total row
    expect(screen.getAllByText('$3.50').length).toBe(2)
  })

  it('displays Active badge for unreturned rental', () => {
    renderWithRouter(<RentalDetail rental={sampleRentalDetail} />)
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('displays Returned badge for returned rental', () => {
    const returned: RentalDetailType = {
      ...sampleRentalDetail,
      returnDate: '2026-02-25T12:00:00',
    }
    renderWithRouter(<RentalDetail rental={returned} />)
    expect(screen.getByText('Returned')).toBeInTheDocument()
  })
})
