import type { ApiError } from '@/types/api'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

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

async function get<T>(
  path: string,
  params?: Record<string, string | undefined>,
): Promise<T> {
  const url = new URL(path, BASE_URL || window.location.origin)

  if (params) {
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined) {
        url.searchParams.set(key, value)
      }
    }
  }

  const response = await fetch(url.toString(), {
    headers: { Accept: 'application/json' },
  })

  return handleResponse<T>(response)
}

async function post<T>(path: string, body: unknown): Promise<T> {
  const url = new URL(path, BASE_URL || window.location.origin)

  const response = await fetch(url.toString(), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(body),
  })

  return handleResponse<T>(response)
}

async function put<T>(path: string, body?: unknown): Promise<T> {
  const url = new URL(path, BASE_URL || window.location.origin)

  const options: RequestInit = {
    method: 'PUT',
    headers: {
      Accept: 'application/json',
    },
  }

  if (body !== undefined) {
    options.headers = {
      ...options.headers,
      'Content-Type': 'application/json',
    }
    options.body = JSON.stringify(body)
  }

  const response = await fetch(url.toString(), options)

  return handleResponse<T>(response)
}

async function del(path: string): Promise<void> {
  const url = new URL(path, BASE_URL || window.location.origin)

  const response = await fetch(url.toString(), {
    method: 'DELETE',
    headers: { Accept: 'application/json' },
  })

  return handleResponse<void>(response)
}

export const api = { get, post, put, del }
