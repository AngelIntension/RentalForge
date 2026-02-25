import type { ApiError } from '@/types/api'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

interface TokenAccessor {
  getToken: () => string | null
  getRefreshToken: () => string | null
  isExpiringSoon: () => boolean
  onRefreshed: (token: string, refreshToken: string) => void
  onRefreshFailed: () => void
}

let tokenAccessor: TokenAccessor | null = null
let refreshPromise: Promise<boolean> | null = null

export function setTokenAccessor(accessor: TokenAccessor) {
  tokenAccessor = accessor
}

async function tryRefreshToken(): Promise<boolean> {
  if (!tokenAccessor) return false
  const refreshToken = tokenAccessor.getRefreshToken()
  if (!refreshToken) return false

  // Deduplicate concurrent refresh attempts
  if (refreshPromise) return refreshPromise

  refreshPromise = (async () => {
    try {
      const url = new URL('/api/auth/refresh', BASE_URL || window.location.origin)
      const response = await fetch(url.toString(), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ refreshToken }),
      })
      if (!response.ok) {
        tokenAccessor!.onRefreshFailed()
        return false
      }
      const data = (await response.json()) as { token: string; refreshToken: string }
      tokenAccessor!.onRefreshed(data.token, data.refreshToken)
      return true
    } catch {
      tokenAccessor!.onRefreshFailed()
      return false
    } finally {
      refreshPromise = null
    }
  })()

  return refreshPromise
}

function getAuthHeaders(): Record<string, string> {
  if (!tokenAccessor) return {}
  const token = tokenAccessor.getToken()
  if (!token) return {}
  return { Authorization: `Bearer ${token}` }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    if (response.status === 204) {
      return undefined as T
    }
    return response.json() as Promise<T>
  }

  let body: Record<string, unknown> = {}
  try {
    body = await response.json()
  } catch {
    // Response may not have a JSON body
  }

  const error: ApiError = {
    status: response.status,
    title: (body.title as string) ?? 'An unexpected error occurred',
    errors: (body.errors as Record<string, string[]>) ?? null,
  }

  throw error
}

async function requestWithRefresh<T>(
  makeFetch: (authHeaders: Record<string, string>) => Promise<Response>,
): Promise<T> {
  // Pre-emptively refresh if token is expiring soon
  if (tokenAccessor?.isExpiringSoon()) {
    await tryRefreshToken()
  }

  const response = await makeFetch(getAuthHeaders())

  // On 401, attempt one refresh then retry
  if (response.status === 401 && tokenAccessor) {
    const refreshed = await tryRefreshToken()
    if (refreshed) {
      const retryResponse = await makeFetch(getAuthHeaders())
      return handleResponse<T>(retryResponse)
    }
  }

  return handleResponse<T>(response)
}

async function get<T>(
  path: string,
  params?: Record<string, string | undefined>,
): Promise<T> {
  return requestWithRefresh<T>((authHeaders) => {
    const url = new URL(path, BASE_URL || window.location.origin)

    if (params) {
      for (const [key, value] of Object.entries(params)) {
        if (value !== undefined) {
          url.searchParams.set(key, value)
        }
      }
    }

    return fetch(url.toString(), {
      headers: { Accept: 'application/json', ...authHeaders },
    })
  })
}

async function post<T>(path: string, body: unknown): Promise<T> {
  return requestWithRefresh<T>((authHeaders) => {
    const url = new URL(path, BASE_URL || window.location.origin)

    return fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        ...authHeaders,
      },
      body: JSON.stringify(body),
    })
  })
}

async function put<T>(path: string, body?: unknown): Promise<T> {
  return requestWithRefresh<T>((authHeaders) => {
    const url = new URL(path, BASE_URL || window.location.origin)

    const options: RequestInit = {
      method: 'PUT',
      headers: {
        Accept: 'application/json',
        ...authHeaders,
      },
    }

    if (body !== undefined) {
      options.headers = {
        ...options.headers,
        'Content-Type': 'application/json',
      }
      options.body = JSON.stringify(body)
    }

    return fetch(url.toString(), options)
  })
}

async function del(path: string): Promise<void> {
  return requestWithRefresh<void>((authHeaders) => {
    const url = new URL(path, BASE_URL || window.location.origin)

    return fetch(url.toString(), {
      method: 'DELETE',
      headers: { Accept: 'application/json', ...authHeaders },
    })
  })
}

export const api = { get, post, put, del }
