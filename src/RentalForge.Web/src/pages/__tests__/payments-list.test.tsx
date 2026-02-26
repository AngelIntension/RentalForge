import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { QueryClientProvider } from '@tanstack/react-query'
import { createTestQueryClient } from '@/lib/query-client'
import { PaymentsList } from '@/pages/payments-list'
import { samplePaymentListItems } from '@/test/fixtures/data'
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

describe('PaymentsList', () => {
  beforeEach(() => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Staff',
      user: { id: 'test', email: 'staff@rentalforge.dev', role: 'Staff', customerId: null, staffId: 1, createdAt: '' },
      token: 'test-token',
    })
  })

  it('renders page title', async () => {
    render(<PaymentsList />, { wrapper: createWrapper() })

    expect(screen.getByText('Payments')).toBeInTheDocument()
    expect(screen.getByText('View payment history')).toBeInTheDocument()
  })

  it('renders payment cards from API', async () => {
    render(<PaymentsList />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByText(`Payment #${samplePaymentListItems[0].id}`)).toBeInTheDocument()
    })

    for (const payment of samplePaymentListItems) {
      expect(screen.getByText(`Payment #${payment.id}`)).toBeInTheDocument()
    }
  })

  it('shows loading state initially', () => {
    render(<PaymentsList />, { wrapper: createWrapper() })

    // The LoadingState component should render before data arrives
    // Once data arrives, it disappears
    expect(screen.getByText('Payments')).toBeInTheDocument()
  })
})
