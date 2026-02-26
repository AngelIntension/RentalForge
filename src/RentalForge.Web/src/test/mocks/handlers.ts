import { http, HttpResponse } from 'msw'
import {
  sampleFilmListItems,
  sampleFilmDetail,
  sampleCustomerListItems,
  sampleRentalListItems,
  sampleRentalDetail,
  sampleReturnedRentalDetail,
  samplePaymentListItems,
  samplePaymentDetail,
  sampleAuthResponse,
  sampleUserDto,
} from '../fixtures/data'
import type { PagedResponse } from '@/types/api'
import type { FilmListItem, FilmDetail } from '@/types/film'
import type { CustomerListItem } from '@/types/customer'
import type { RentalListItem, RentalDetail } from '@/types/rental'
import type { PaymentListItem, PaymentDetail } from '@/types/payment'

function paginate<T>(items: T[], url: URL): PagedResponse<T> {
  const page = Number(url.searchParams.get('page') ?? '1')
  const pageSize = Number(url.searchParams.get('pageSize') ?? '10')
  const totalCount = items.length
  const totalPages = Math.ceil(totalCount / pageSize)
  const start = (page - 1) * pageSize
  const paged = items.slice(start, start + pageSize)

  return { items: paged, page, pageSize, totalCount, totalPages }
}

// Note: MSW handlers use regex patterns because jsdom constructs fully-qualified
// URLs (e.g., http://localhost:5089/api/films) which don't match string-path
// handlers (e.g., '/api/films'). Regex patterns match against the full URL.

export const handlers = [
  // Health
  http.get(/\/health$/, () => {
    return HttpResponse.json({ status: 'Healthy' })
  }),

  // Films - list
  http.get(/\/api\/films(\?|$)/, ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<FilmListItem> = paginate(sampleFilmListItems, url)
    return HttpResponse.json(response)
  }),

  // Films - detail
  http.get(/\/api\/films\/(\d+)$/, ({ request }) => {
    const id = Number(request.url.match(/\/api\/films\/(\d+)/)?.[1])
    if (id === sampleFilmDetail.id) {
      return HttpResponse.json(sampleFilmDetail satisfies FilmDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Films - create
  http.post(/\/api\/films$/, async () => {
    return HttpResponse.json(sampleFilmDetail satisfies FilmDetail, { status: 201 })
  }),

  // Customers - list
  http.get(/\/api\/customers(\?|$)/, ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<CustomerListItem> = paginate(sampleCustomerListItems, url)
    return HttpResponse.json(response)
  }),

  // Customers - detail
  http.get(/\/api\/customers\/(\d+)$/, ({ request }) => {
    const id = Number(request.url.match(/\/api\/customers\/(\d+)/)?.[1])
    const customer = sampleCustomerListItems.find((c) => c.id === id)
    if (customer) {
      return HttpResponse.json(customer satisfies CustomerListItem)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Rentals - list
  http.get(/\/api\/rentals(\?|$)/, ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<RentalListItem> = paginate(sampleRentalListItems, url)
    return HttpResponse.json(response)
  }),

  // Rentals - detail
  http.get(/\/api\/rentals\/(\d+)$/, ({ request }) => {
    const id = Number(request.url.match(/\/api\/rentals\/(\d+)/)?.[1])
    if (id === sampleRentalDetail.id) {
      return HttpResponse.json(sampleRentalDetail satisfies RentalDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Rentals - create
  http.post(/\/api\/rentals$/, async () => {
    return HttpResponse.json(sampleRentalDetail satisfies RentalDetail, { status: 201 })
  }),

  // Rentals - return (accepts optional body with amount + staffId)
  http.put(/\/api\/rentals\/(\d+)\/return$/, async ({ request }) => {
    const id = Number(request.url.match(/\/api\/rentals\/(\d+)\/return/)?.[1])
    if (id === sampleReturnedRentalDetail.id) {
      return HttpResponse.json(sampleReturnedRentalDetail satisfies RentalDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Payments - list
  http.get(/\/api\/payments(\?|$)/, ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<PaymentListItem> = paginate(samplePaymentListItems, url)
    return HttpResponse.json(response)
  }),

  // Payments - create
  http.post(/\/api\/payments$/, async () => {
    return HttpResponse.json(samplePaymentDetail satisfies PaymentDetail, { status: 201 })
  }),

  // Auth - register
  http.post(/\/api\/auth\/register$/, async ({ request }) => {
    const body = (await request.json()) as { email?: string; password?: string }
    if (!body.email || !body.password) {
      return HttpResponse.json(
        { title: 'One or more validation errors occurred.', status: 400, errors: { email: ['Email is required.'] } },
        { status: 400 },
      )
    }
    if (body.email === 'existing@example.com') {
      return HttpResponse.json(
        { title: 'One or more validation errors occurred.', status: 400, errors: { email: ['Email is already registered.'] } },
        { status: 400 },
      )
    }
    return HttpResponse.json(sampleAuthResponse, { status: 201 })
  }),

  // Auth - login
  http.post(/\/api\/auth\/login$/, async ({ request }) => {
    const body = (await request.json()) as { email?: string; password?: string }
    if (body.email === 'staff@rentalforge.dev' && body.password === 'RentalForge1!') {
      return HttpResponse.json(sampleAuthResponse)
    }
    return HttpResponse.json(
      { title: 'Invalid email or password.', status: 401 },
      { status: 401 },
    )
  }),

  // Auth - logout
  http.post(/\/api\/auth\/logout$/, () => {
    return new HttpResponse(null, { status: 204 })
  }),

  // Auth - refresh
  http.post(/\/api\/auth\/refresh$/, async ({ request }) => {
    const body = (await request.json()) as { refreshToken?: string }
    if (body.refreshToken === 'valid-refresh-token') {
      return HttpResponse.json({
        token: sampleAuthResponse.token,
        refreshToken: 'new-refresh-token',
      })
    }
    return HttpResponse.json(
      { title: 'Invalid refresh token.', status: 401 },
      { status: 401 },
    )
  }),

  // Auth - me
  http.get(/\/api\/auth\/me$/, ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    if (!authHeader?.startsWith('Bearer ')) {
      return HttpResponse.json({ title: 'Unauthorized', status: 401 }, { status: 401 })
    }
    return HttpResponse.json(sampleUserDto)
  }),
]
