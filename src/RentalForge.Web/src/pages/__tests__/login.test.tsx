import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen, userEvent, waitFor } from '@/test/test-utils'
import { Login } from '@/pages/login'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'
import { sampleAuthResponse } from '@/test/fixtures/data'

function loginHandler(success = true) {
  return http.post(/\/api\/auth\/login/, () => {
    if (success) {
      return HttpResponse.json(sampleAuthResponse)
    }
    return HttpResponse.json(
      { title: 'Invalid email or password.', status: 401 },
      { status: 401 },
    )
  })
}

describe('Login page', () => {
  beforeEach(() => {
    server.use(loginHandler())
  })

  it('renders form fields', () => {
    render(<Login />)

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('shows validation errors on empty submit', async () => {
    render(<Login />)

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByText(/valid email/i)).toBeInTheDocument()
    })
  })

  it('submits to API on valid form without showing errors', async () => {
    render(<Login />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'staff@rentalforge.dev')
    await user.type(screen.getByLabelText(/password/i), 'RentalForge1!')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    // Successful login should not show any error messages
    await waitFor(() => {
      expect(screen.queryByText(/invalid email or password/i)).not.toBeInTheDocument()
      expect(screen.queryByText(/login failed/i)).not.toBeInTheDocument()
    })
  })

  it('shows server error on invalid credentials', async () => {
    server.use(loginHandler(false))

    render(<Login />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'wrong@example.com')
    await user.type(screen.getByLabelText(/password/i), 'WrongP@ss1')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByText(/invalid email or password/i)).toBeInTheDocument()
    })
  })

  it('shows link to register page', () => {
    render(<Login />)

    const registerLink = screen.getByRole('link', { name: /create an account/i })
    expect(registerLink).toHaveAttribute('href', '/register')
  })
})
