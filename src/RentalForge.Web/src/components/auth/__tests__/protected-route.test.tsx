import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { ProtectedRoute } from '@/components/auth/protected-route'

const mockUseAuth = vi.fn()

vi.mock('@/hooks/use-auth', () => ({
  useAuth: () => mockUseAuth(),
}))

function renderWithRouter(ui: React.ReactElement) {
  return render(<MemoryRouter>{ui}</MemoryRouter>)
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    mockUseAuth.mockReset()
  })

  it('renders nothing while loading', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
      role: null,
    })

    const { container } = renderWithRouter(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
    )

    expect(container.innerHTML).toBe('')
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
  })

  it('redirects to /login when not authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      role: null,
    })

    renderWithRouter(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
    )

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
  })

  it('renders children when authenticated', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Staff',
    })

    renderWithRouter(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
    )

    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })

  it('shows access denied when role is insufficient', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Customer',
    })

    renderWithRouter(
      <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
        <div>Staff Only Content</div>
      </ProtectedRoute>,
    )

    expect(screen.getByText(/access denied/i)).toBeInTheDocument()
    expect(screen.queryByText('Staff Only Content')).not.toBeInTheDocument()
  })

  it('renders children when role is in allowedRoles', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Admin',
    })

    renderWithRouter(
      <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
        <div>Admin Content</div>
      </ProtectedRoute>,
    )

    expect(screen.getByText('Admin Content')).toBeInTheDocument()
  })

  it('renders children when no allowedRoles specified', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      role: 'Customer',
    })

    renderWithRouter(
      <ProtectedRoute>
        <div>Any Authenticated Content</div>
      </ProtectedRoute>,
    )

    expect(screen.getByText('Any Authenticated Content')).toBeInTheDocument()
  })
})
