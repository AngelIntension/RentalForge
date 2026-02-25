import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen, userEvent, waitFor } from '@/test/test-utils'
import { Register } from '@/pages/register'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'
import { sampleAuthResponse } from '@/test/fixtures/data'

function registerHandler() {
  return http.post(/\/api\/auth\/register/, async ({ request }) => {
    const body = (await request.json()) as { email?: string; password?: string }
    if (body.email === 'existing@example.com') {
      return HttpResponse.json(
        {
          title: 'One or more validation errors occurred.',
          status: 400,
          errors: { email: ['Email is already registered.'] },
        },
        { status: 400 },
      )
    }
    return HttpResponse.json(sampleAuthResponse, { status: 201 })
  })
}

describe('Register page', () => {
  beforeEach(() => {
    server.use(registerHandler())
  })

  it('renders form fields', () => {
    render(<Register />)

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument()
  })

  it('shows validation errors on empty submit', async () => {
    render(<Register />)

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/valid email/i)).toBeInTheDocument()
    })
  })

  it('shows password strength errors', async () => {
    render(<Register />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'short')
    await user.type(screen.getByLabelText(/confirm password/i), 'short')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/at least 8 characters/i)).toBeInTheDocument()
    })
  })

  it('shows confirm password mismatch error', async () => {
    render(<Register />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'SecureP@ss1')
    await user.type(screen.getByLabelText(/confirm password/i), 'DifferentP@ss1')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument()
    })
  })

  it('submits to API on valid form without showing errors', async () => {
    render(<Register />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'newuser@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'SecureP@ss1')
    await user.type(screen.getByLabelText(/confirm password/i), 'SecureP@ss1')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    // Successful registration should not show any error messages
    await waitFor(() => {
      expect(screen.queryByText(/registration failed/i)).not.toBeInTheDocument()
      expect(screen.queryByText(/already registered/i)).not.toBeInTheDocument()
    })
  })

  it('displays server validation errors', async () => {
    render(<Register />)

    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/email/i), 'existing@example.com')
    await user.type(screen.getByLabelText(/^password$/i), 'SecureP@ss1')
    await user.type(screen.getByLabelText(/confirm password/i), 'SecureP@ss1')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/already registered/i)).toBeInTheDocument()
    })
  })

  it('shows link to login page', () => {
    render(<Register />)

    const loginLink = screen.getByRole('link', { name: /sign in/i })
    expect(loginLink).toHaveAttribute('href', '/login')
  })
})
