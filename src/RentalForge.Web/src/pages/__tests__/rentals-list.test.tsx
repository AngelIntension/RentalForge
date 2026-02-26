import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { QueryClientProvider } from '@tanstack/react-query'
import { createTestQueryClient } from '@/lib/query-client'
import { RentalsList } from '@/pages/rentals-list'
import { sampleRentalListItems } from '@/test/fixtures/data'
import type { ReactNode } from 'react'

const mockUseAuth = vi.fn()

vi.mock('@/hooks/use-auth', () => ({
  useAuth: () => mockUseAuth(),
}))

function createWrapper() {
  const queryClient = createTestQueryClient()
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>{children}</MemoryRouter>
      </QueryClientProvider>
    )
  }
}

describe('RentalsList', () => {
  beforeEach(() => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Staff',
      user: { id: 'test', email: 'staff@rentalforge.dev', role: 'Staff', customerId: null, staffId: 1, createdAt: '' },
      token: 'test-token',
    })
  })

  it('renders page title and new rental button', async () => {
    render(<RentalsList />, { wrapper: createWrapper() })

    expect(screen.getByText('Rentals')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /new rental/i })).toBeInTheDocument()
  })

  it('renders rental cards from API', async () => {
    render(<RentalsList />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByText(`Rental #${sampleRentalListItems[0].id}`)).toBeInTheDocument()
    })

    for (const rental of sampleRentalListItems) {
      expect(screen.getByText(`Rental #${rental.id}`)).toBeInTheDocument()
    }
  })

  it('shows Return & Pay button for Staff on active rental', async () => {
    render(<RentalsList />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByText(`Rental #${sampleRentalListItems[0].id}`)).toBeInTheDocument()
    })

    // Active rentals should have Return & Pay buttons for Staff
    const returnPayButtons = screen.getAllByRole('button', { name: /return & pay/i })
    expect(returnPayButtons.length).toBeGreaterThan(0)
  })

  it('opens Return & Pay modal when button clicked', async () => {
    render(<RentalsList />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByText(`Rental #${sampleRentalListItems[0].id}`)).toBeInTheDocument()
    })

    const user = userEvent.setup()
    const returnPayButtons = screen.getAllByRole('button', { name: /return & pay/i })
    await user.click(returnPayButtons[0])

    // Modal should open
    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument()
    })
    expect(screen.getByLabelText(/amount/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/staff id/i)).toBeInTheDocument()
  })

  it('shows Return button for Customer on active rental', async () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Customer',
      user: { id: 'test', email: 'customer@example.com', role: 'Customer', customerId: 130, staffId: null, createdAt: '' },
      token: 'test-token',
    })

    render(<RentalsList />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByText(`Rental #${sampleRentalListItems[0].id}`)).toBeInTheDocument()
    })

    // Customers should see "Return" not "Return & Pay"
    expect(screen.queryByRole('button', { name: /return & pay/i })).not.toBeInTheDocument()
  })

  it('has active only filter checkbox', () => {
    render(<RentalsList />, { wrapper: createWrapper() })

    expect(screen.getByLabelText(/active only/i)).toBeInTheDocument()
  })
})
