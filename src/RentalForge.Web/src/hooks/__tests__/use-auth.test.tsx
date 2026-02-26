import { describe, it, expect, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/hooks/use-auth'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'
import { sampleAuthResponse, createTestJwt, sampleUserDto } from '@/test/fixtures/data'

function wrapper({ children }: { children: React.ReactNode }) {
  return <AuthProvider>{children}</AuthProvider>
}

function setupAuthHandlers() {
  server.use(
    http.post(/\/api\/auth\/login/, () => {
      return HttpResponse.json(sampleAuthResponse)
    }),
    http.post(/\/api\/auth\/register/, () => {
      return HttpResponse.json(sampleAuthResponse, { status: 201 })
    }),
    http.post(/\/api\/auth\/logout/, () => {
      return new HttpResponse(null, { status: 204 })
    }),
    http.post(/\/api\/auth\/refresh/, () => {
      return HttpResponse.json({
        token: createTestJwt(),
        refreshToken: 'new-refresh-token',
      })
    }),
  )
}

describe('useAuth', () => {
  beforeEach(() => {
    setupAuthHandlers()
  })

  it('starts unauthenticated', async () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.user).toBeNull()
    expect(result.current.token).toBeNull()
    expect(result.current.role).toBeNull()
  })

  it('login sets auth state', async () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    await act(async () => {
      await result.current.login({
        email: 'staff@rentalforge.dev',
        password: 'RentalForge1!',
      })
    })

    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.user?.email).toBe(sampleUserDto.email)
    expect(result.current.user?.staffId).toBe(sampleUserDto.staffId)
    expect(result.current.role).toBe(sampleUserDto.role)
    expect(result.current.token).toBeTruthy()
  })

  it('login populates null staffId for non-staff user', async () => {
    const customerUserDto = { ...sampleUserDto, role: 'Customer' as const, customerId: 42, staffId: null }
    const customerJwt = createTestJwt({ role: 'Customer' })
    const customerAuthResponse = {
      token: customerJwt,
      refreshToken: 'customer-refresh-token',
      user: customerUserDto,
    }

    server.use(
      http.post(/\/api\/auth\/login/, () => {
        return HttpResponse.json(customerAuthResponse)
      }),
    )

    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    await act(async () => {
      await result.current.login({
        email: 'customer@rentalforge.dev',
        password: 'RentalForge1!',
      })
    })

    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.user?.staffId).toBeNull()
  })

  it('logout clears auth state', async () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    // Login first
    await act(async () => {
      await result.current.login({
        email: 'staff@rentalforge.dev',
        password: 'RentalForge1!',
      })
    })

    expect(result.current.isAuthenticated).toBe(true)

    // Now logout
    await act(async () => {
      await result.current.logout()
    })

    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.user).toBeNull()
    expect(result.current.token).toBeNull()
  })

  it('register sets auth state', async () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    await act(async () => {
      await result.current.register({
        email: 'new@example.com',
        password: 'SecureP@ss1',
      })
    })

    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.user).not.toBeNull()
  })

  it('throws when used outside AuthProvider', () => {
    expect(() => {
      renderHook(() => useAuth())
    }).toThrow('useAuth must be used within an AuthProvider')
  })
})
