import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, setTokenAccessor } from '@/lib/api-client'
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserDto,
} from '@/types/auth'

const STORAGE_KEY_TOKEN = 'rentalforge-token'
const STORAGE_KEY_REFRESH = 'rentalforge-refresh-token'

interface AuthState {
  user: UserDto | null
  token: string | null
  refreshToken: string | null
}

interface AuthContextValue {
  user: UserDto | null
  token: string | null
  role: string | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (request: LoginRequest) => Promise<void>
  register: (request: RegisterRequest) => Promise<AuthResponse>
  logout: () => Promise<void>
  refresh: () => Promise<boolean>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) return null
    const payload = parts[1]
    const padded = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = atob(padded)
    return JSON.parse(decoded) as Record<string, unknown>
  } catch {
    return null
  }
}

function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token)
  if (!payload || typeof payload.exp !== 'number') return true
  return Date.now() >= payload.exp * 1000
}

function isTokenExpiringSoon(token: string, thresholdMs = 60_000): boolean {
  const payload = decodeJwtPayload(token)
  if (!payload || typeof payload.exp !== 'number') return true
  return Date.now() >= payload.exp * 1000 - thresholdMs
}

function userFromToken(token: string): UserDto | null {
  const payload = decodeJwtPayload(token)
  if (!payload) return null
  return {
    id: payload.sub as string,
    email: payload.email as string,
    role: payload.role as UserDto['role'],
    customerId: (payload.customerId as number) ?? null,
    createdAt: '',
  }
}

const emptyState: AuthState = { user: null, token: null, refreshToken: null }

function loadFromStorage(): AuthState {
  try {
    const token = localStorage.getItem(STORAGE_KEY_TOKEN)
    const refreshToken = localStorage.getItem(STORAGE_KEY_REFRESH)
    if (!token) return emptyState
    const user = userFromToken(token)
    return { user, token, refreshToken }
  } catch {
    return emptyState
  }
}

function saveToStorage(token: string, refreshToken: string) {
  try {
    localStorage.setItem(STORAGE_KEY_TOKEN, token)
    localStorage.setItem(STORAGE_KEY_REFRESH, refreshToken)
  } catch { /* ignore storage errors */ }
}

function clearStorage() {
  try {
    localStorage.removeItem(STORAGE_KEY_TOKEN)
    localStorage.removeItem(STORAGE_KEY_REFRESH)
  } catch { /* ignore storage errors */ }
}

function readRefreshFromStorage(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY_REFRESH)
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    refreshToken: null,
  })
  const [isLoading, setIsLoading] = useState(true)

  const setAuth = useCallback((token: string, refreshToken: string, user: UserDto) => {
    saveToStorage(token, refreshToken)
    setState({ user, token, refreshToken })
  }, [])

  const clearAuth = useCallback(() => {
    clearStorage()
    setState({ user: null, token: null, refreshToken: null })
  }, [])

  const refresh = useCallback(async (): Promise<boolean> => {
    const storedRefresh = state.refreshToken ?? localStorage.getItem(STORAGE_KEY_REFRESH)
    if (!storedRefresh) return false

    try {
      const response = await api.post<{ token: string; refreshToken: string }>(
        '/api/auth/refresh',
        { refreshToken: storedRefresh },
      )
      const user = userFromToken(response.token)
      if (user) {
        setAuth(response.token, response.refreshToken, user)
      }
      return true
    } catch {
      clearAuth()
      return false
    }
  }, [state.refreshToken, setAuth, clearAuth])

  const login = useCallback(async (request: LoginRequest) => {
    const response = await api.post<AuthResponse>('/api/auth/login', request)
    setAuth(response.token, response.refreshToken, response.user)
  }, [setAuth])

  const register = useCallback(async (request: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/register', request)
    setAuth(response.token, response.refreshToken, response.user)
    return response
  }, [setAuth])

  const logout = useCallback(async () => {
    const currentRefresh = state.refreshToken
    clearAuth()
    if (currentRefresh) {
      try {
        await api.post('/api/auth/logout', { refreshToken: currentRefresh })
      } catch {
        // Logout is best-effort — already cleared local state
      }
    }
  }, [state.refreshToken, clearAuth])

  // Initialize from localStorage on mount
  useEffect(() => {
    let cancelled = false

    const init = async () => {
      const stored = loadFromStorage()
      if (!stored.token) {
        if (!cancelled) setIsLoading(false)
        return
      }

      if (!isTokenExpired(stored.token)) {
        if (!cancelled) {
          setState(stored)
          setIsLoading(false)
        }
        return
      }

      // Token expired — try refresh
      if (stored.refreshToken) {
        try {
          const response = await api.post<{ token: string; refreshToken: string }>(
            '/api/auth/refresh',
            { refreshToken: stored.refreshToken },
          )
          if (!cancelled) {
            const user = userFromToken(response.token)
            if (user) {
              saveToStorage(response.token, response.refreshToken)
              setState({ user, token: response.token, refreshToken: response.refreshToken })
            } else {
              clearStorage()
            }
          }
        } catch {
          if (!cancelled) clearStorage()
        }
      } else {
        if (!cancelled) clearStorage()
      }
      if (!cancelled) setIsLoading(false)
    }
    init()

    return () => { cancelled = true }
  }, [])

  // Register token accessor for api-client
  useEffect(() => {
    setTokenAccessor({
      getToken: () => state.token,
      getRefreshToken: () => state.refreshToken ?? readRefreshFromStorage(),
      isExpiringSoon: () => (state.token ? isTokenExpiringSoon(state.token) : false),
      onRefreshed: (token, refreshToken) => {
        const user = userFromToken(token)
        if (user) {
          setAuth(token, refreshToken, user)
        }
      },
      onRefreshFailed: () => {
        clearAuth()
      },
    })
  }, [state.token, state.refreshToken, setAuth, clearAuth])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: state.user,
      token: state.token,
      role: state.user?.role ?? null,
      isAuthenticated: state.user !== null,
      isLoading,
      login,
      register,
      logout,
      refresh,
    }),
    [state.user, state.token, isLoading, login, register, logout, refresh],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
