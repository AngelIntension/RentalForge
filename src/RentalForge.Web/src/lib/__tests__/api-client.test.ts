import { describe, it, expect, beforeEach, vi } from 'vitest'
import { http, HttpResponse } from 'msw'
import { server } from '@/test/mocks/server'
import { api, setTokenAccessor } from '@/lib/api-client'
import { createTestJwt } from '@/test/fixtures/data'

describe('api-client', () => {
  beforeEach(() => {
    // Reset any runtime handlers added during tests
    server.resetHandlers()
    // Reset token accessor between tests
    setTokenAccessor(null as never)
  })

  describe('get', () => {
    it('fetches JSON data', async () => {
      server.use(
        http.get(/\/api\/test($|\?)/, () => {
          return HttpResponse.json({ message: 'hello' })
        }),
      )

      const result = await api.get<{ message: string }>('/api/test')
      expect(result).toEqual({ message: 'hello' })
    })

    it('appends query params', async () => {
      server.use(
        http.get(/\/api\/test($|\?)/, ({ request }) => {
          const url = new URL(request.url)
          return HttpResponse.json({
            search: url.searchParams.get('search'),
            page: url.searchParams.get('page'),
          })
        }),
      )

      const result = await api.get<{ search: string; page: string }>(
        '/api/test',
        { search: 'foo', page: '2' },
      )
      expect(result).toEqual({ search: 'foo', page: '2' })
    })

    it('omits undefined query params', async () => {
      server.use(
        http.get(/\/api\/test($|\?)/, ({ request }) => {
          const url = new URL(request.url)
          return HttpResponse.json({
            params: Object.fromEntries(url.searchParams.entries()),
          })
        }),
      )

      const result = await api.get<{ params: Record<string, string> }>(
        '/api/test',
        { search: 'foo', category: undefined as unknown as string },
      )
      expect(result.params).toEqual({ search: 'foo' })
    })
  })

  describe('post', () => {
    it('sends JSON body and returns response', async () => {
      server.use(
        http.post(/\/api\/test$/, async ({ request }) => {
          const body = await request.json()
          return HttpResponse.json(body, { status: 201 })
        }),
      )

      const result = await api.post<{ name: string }>('/api/test', {
        name: 'test',
      })
      expect(result).toEqual({ name: 'test' })
    })
  })

  describe('put', () => {
    it('sends PUT with JSON body', async () => {
      server.use(
        http.put(/\/api\/test\/\d+$/, async ({ request }) => {
          const body = await request.json()
          return HttpResponse.json(body)
        }),
      )

      const result = await api.put<{ name: string }>('/api/test/1', {
        name: 'updated',
      })
      expect(result).toEqual({ name: 'updated' })
    })

    it('sends PUT without body', async () => {
      server.use(
        http.put(/\/api\/test\/\d+\/action$/, () => {
          return HttpResponse.json({ done: true })
        }),
      )

      const result = await api.put<{ done: boolean }>('/api/test/1/action')
      expect(result).toEqual({ done: true })
    })
  })

  describe('del', () => {
    it('sends DELETE and returns void', async () => {
      server.use(
        http.delete(/\/api\/test\/\d+$/, () => {
          return new HttpResponse(null, { status: 204 })
        }),
      )

      await expect(api.del('/api/test/1')).resolves.toBeUndefined()
    })
  })

  describe('error handling', () => {
    it('throws ApiError with validation errors on 400', async () => {
      server.use(
        http.post(/\/api\/test$/, () => {
          return HttpResponse.json(
            {
              type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
              title: 'One or more validation errors occurred.',
              status: 400,
              errors: {
                Name: ['Name is required'],
                Email: ['Email is invalid', 'Email is too long'],
              },
            },
            { status: 400 },
          )
        }),
      )

      try {
        await api.post('/api/test', {})
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toMatchObject({
          status: 400,
          title: 'One or more validation errors occurred.',
          errors: {
            Name: ['Name is required'],
            Email: ['Email is invalid', 'Email is too long'],
          },
        })
      }
    })

    it('throws ApiError with status and title on 404', async () => {
      server.use(
        http.get(/\/api\/test\/999$/, () => {
          return HttpResponse.json(
            { title: 'Not Found', status: 404 },
            { status: 404 },
          )
        }),
      )

      try {
        await api.get('/api/test/999')
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toMatchObject({
          status: 404,
          title: 'Not Found',
          errors: null,
        })
      }
    })

    it('throws ApiError with conflict title on 409', async () => {
      server.use(
        http.delete(/\/api\/test\/1$/, () => {
          return HttpResponse.json(
            { title: 'Film has active rentals', status: 409 },
            { status: 409 },
          )
        }),
      )

      try {
        await api.del('/api/test/1')
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toMatchObject({
          status: 409,
          title: 'Film has active rentals',
          errors: null,
        })
      }
    })

    it('throws ApiError with generic title on 500', async () => {
      server.use(
        http.get(/\/api\/test$/, () => {
          return new HttpResponse(null, { status: 500 })
        }),
      )

      try {
        await api.get('/api/test')
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toMatchObject({
          status: 500,
          title: 'An unexpected error occurred',
          errors: null,
        })
      }
    })
  })

  describe('base URL', () => {
    it('uses VITE_API_BASE_URL from env', async () => {
      // The default in tests is empty, so requests go to relative URLs
      // which MSW intercepts. This test verifies the client constructs
      // proper URLs.
      server.use(
        http.get(/\/api\/test$/, () => {
          return HttpResponse.json({ ok: true })
        }),
      )

      const result = await api.get<{ ok: boolean }>('/api/test')
      expect(result.ok).toBe(true)
    })
  })

  describe('token refresh', () => {
    function setupTokenAccessor(overrides: Partial<{
      token: string | null
      refreshToken: string | null
      expiringSoon: boolean
    }> = {}) {
      const onRefreshed = vi.fn()
      const onRefreshFailed = vi.fn()

      const accessor = {
        getToken: () => overrides.token ?? createTestJwt(),
        getRefreshToken: () => overrides.refreshToken ?? 'valid-refresh-token',
        isExpiringSoon: () => overrides.expiringSoon ?? false,
        onRefreshed,
        onRefreshFailed,
      }

      setTokenAccessor(accessor)
      return { onRefreshed, onRefreshFailed, accessor }
    }

    it('adds Authorization header when token accessor is set', async () => {
      const token = createTestJwt()
      setupTokenAccessor({ token })

      server.use(
        http.get(/\/api\/protected/, ({ request }) => {
          const auth = request.headers.get('Authorization')
          return HttpResponse.json({ auth })
        }),
      )

      const result = await api.get<{ auth: string }>('/api/protected')
      expect(result.auth).toBe(`Bearer ${token}`)
    })

    it('pre-emptively refreshes when token is expiring soon', async () => {
      const newToken = createTestJwt()
      const { onRefreshed } = setupTokenAccessor({ expiringSoon: true })

      server.use(
        http.post(/\/api\/auth\/refresh/, () => {
          return HttpResponse.json({
            token: newToken,
            refreshToken: 'new-refresh-token',
          })
        }),
        http.get(/\/api\/data/, () => {
          return HttpResponse.json({ value: 42 })
        }),
      )

      const result = await api.get<{ value: number }>('/api/data')
      expect(result.value).toBe(42)
      expect(onRefreshed).toHaveBeenCalledWith(newToken, 'new-refresh-token')
    })

    it('retries request after 401 with successful refresh', async () => {
      let callCount = 0
      const { onRefreshed } = setupTokenAccessor({ expiringSoon: false })

      server.use(
        http.get(/\/api\/secured/, () => {
          callCount++
          if (callCount === 1) {
            return HttpResponse.json(
              { title: 'Unauthorized', status: 401 },
              { status: 401 },
            )
          }
          return HttpResponse.json({ data: 'success' })
        }),
        http.post(/\/api\/auth\/refresh/, () => {
          return HttpResponse.json({
            token: createTestJwt(),
            refreshToken: 'refreshed-token',
          })
        }),
      )

      const result = await api.get<{ data: string }>('/api/secured')
      expect(result.data).toBe('success')
      expect(callCount).toBe(2)
      expect(onRefreshed).toHaveBeenCalled()
    })

    it('calls onRefreshFailed when refresh returns 401', async () => {
      const { onRefreshFailed } = setupTokenAccessor({ expiringSoon: false })

      server.use(
        http.get(/\/api\/secured/, () => {
          return HttpResponse.json(
            { title: 'Unauthorized', status: 401 },
            { status: 401 },
          )
        }),
        http.post(/\/api\/auth\/refresh/, () => {
          return HttpResponse.json(
            { title: 'Invalid refresh token.', status: 401 },
            { status: 401 },
          )
        }),
      )

      try {
        await api.get('/api/secured')
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toMatchObject({ status: 401 })
      }

      expect(onRefreshFailed).toHaveBeenCalled()
    })

    it('calls onRefreshFailed when pre-emptive refresh fails', async () => {
      const { onRefreshFailed } = setupTokenAccessor({ expiringSoon: true })

      server.use(
        http.post(/\/api\/auth\/refresh/, () => {
          return HttpResponse.json(
            { title: 'Invalid refresh token.', status: 401 },
            { status: 401 },
          )
        }),
        http.get(/\/api\/data/, () => {
          return HttpResponse.json({ value: 42 })
        }),
      )

      // Pre-emptive refresh fails, but request still proceeds
      const result = await api.get<{ value: number }>('/api/data')
      expect(result.value).toBe(42)
      expect(onRefreshFailed).toHaveBeenCalled()
    })

    it('does not add Authorization header when no token accessor', async () => {
      setTokenAccessor(null as never)

      server.use(
        http.get(/\/api\/public/, ({ request }) => {
          const auth = request.headers.get('Authorization')
          return HttpResponse.json({ auth })
        }),
      )

      const result = await api.get<{ auth: string | null }>('/api/public')
      expect(result.auth).toBeNull()
    })
  })
})
