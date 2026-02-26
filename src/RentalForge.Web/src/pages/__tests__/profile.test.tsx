import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'
import { Profile } from '@/pages/profile'
import { createTestQueryClient } from '@/lib/query-client'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'
import { sampleCustomerListItem } from '@/test/fixtures/data'

const mockUseAuth = vi.fn()

vi.mock('@/hooks/use-auth', () => ({
  useAuth: () => mockUseAuth(),
}))

function renderProfile() {
  const queryClient = createTestQueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <Profile />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('Profile', () => {
  beforeEach(() => {
    mockUseAuth.mockReset()
  })

  it('renders auth user data', () => {
    mockUseAuth.mockReturnValue({
      user: {
        id: 'user-123',
        email: 'staff@rentalforge.dev',
        role: 'Staff',
        customerId: null,
        staffId: 1,
        createdAt: '2026-02-24T10:00:00Z',
      },
      isAuthenticated: true,
      isLoading: false,
      role: 'Staff',
    })

    renderProfile()

    expect(screen.getByText('staff@rentalforge.dev')).toBeInTheDocument()
    expect(screen.getByText('Staff')).toBeInTheDocument()
    expect(screen.getByText(/2026/)).toBeInTheDocument()
  })

  it('fetches and displays linked customer data when customerId present', async () => {
    mockUseAuth.mockReturnValue({
      user: {
        id: 'user-456',
        email: 'customer@rentalforge.dev',
        role: 'Customer',
        customerId: 1,
        staffId: null,
        createdAt: '2026-02-24T10:00:00Z',
      },
      isAuthenticated: true,
      isLoading: false,
      role: 'Customer',
    })

    server.use(
      http.get(/\/api\/customers\/1$/, () => {
        return HttpResponse.json(sampleCustomerListItem)
      }),
    )

    renderProfile()

    await waitFor(() => {
      expect(screen.getByText('Mary')).toBeInTheDocument()
    })
    expect(screen.getByText('Smith')).toBeInTheDocument()
  })

  it('shows no linked customer message when customerId is null', () => {
    mockUseAuth.mockReturnValue({
      user: {
        id: 'user-789',
        email: 'unlinked@rentalforge.dev',
        role: 'Customer',
        customerId: null,
        staffId: null,
        createdAt: '2026-02-24T10:00:00Z',
      },
      isAuthenticated: true,
      isLoading: false,
      role: 'Customer',
    })

    renderProfile()

    expect(screen.getByText(/no linked customer/i)).toBeInTheDocument()
  })

  it('does not show customer section for non-customer roles', () => {
    mockUseAuth.mockReturnValue({
      user: {
        id: 'user-admin',
        email: 'admin@rentalforge.dev',
        role: 'Admin',
        customerId: null,
        staffId: null,
        createdAt: '2026-02-24T10:00:00Z',
      },
      isAuthenticated: true,
      isLoading: false,
      role: 'Admin',
    })

    renderProfile()

    expect(screen.queryByText(/no linked customer/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/customer details/i)).not.toBeInTheDocument()
  })
})
