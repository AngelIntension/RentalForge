import { http, HttpResponse } from 'msw'
import {
  sampleFilmListItems,
  sampleFilmDetail,
  sampleCustomerListItems,
  sampleRentalListItems,
  sampleRentalDetail,
  sampleReturnedRentalDetail,
} from '../fixtures/data'
import type { PagedResponse } from '@/types/api'
import type { FilmListItem, FilmDetail } from '@/types/film'
import type { CustomerListItem } from '@/types/customer'
import type { RentalListItem, RentalDetail } from '@/types/rental'

function paginate<T>(items: T[], url: URL): PagedResponse<T> {
  const page = Number(url.searchParams.get('page') ?? '1')
  const pageSize = Number(url.searchParams.get('pageSize') ?? '10')
  const totalCount = items.length
  const totalPages = Math.ceil(totalCount / pageSize)
  const start = (page - 1) * pageSize
  const paged = items.slice(start, start + pageSize)

  return { items: paged, page, pageSize, totalCount, totalPages }
}

export const handlers = [
  // Health
  http.get('/health', () => {
    return HttpResponse.json({ status: 'Healthy' })
  }),

  // Films - list
  http.get('/api/films', ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<FilmListItem> = paginate(sampleFilmListItems, url)
    return HttpResponse.json(response)
  }),

  // Films - detail
  http.get('/api/films/:id', ({ params }) => {
    const id = Number(params.id)
    if (id === sampleFilmDetail.id) {
      return HttpResponse.json(sampleFilmDetail satisfies FilmDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Films - create
  http.post('/api/films', async () => {
    return HttpResponse.json(sampleFilmDetail satisfies FilmDetail, { status: 201 })
  }),

  // Customers - list
  http.get('/api/customers', ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<CustomerListItem> = paginate(sampleCustomerListItems, url)
    return HttpResponse.json(response)
  }),

  // Customers - detail
  http.get('/api/customers/:id', ({ params }) => {
    const id = Number(params.id)
    const customer = sampleCustomerListItems.find((c) => c.id === id)
    if (customer) {
      return HttpResponse.json(customer satisfies CustomerListItem)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Rentals - list
  http.get('/api/rentals', ({ request }) => {
    const url = new URL(request.url)
    const response: PagedResponse<RentalListItem> = paginate(sampleRentalListItems, url)
    return HttpResponse.json(response)
  }),

  // Rentals - detail
  http.get('/api/rentals/:id', ({ params }) => {
    const id = Number(params.id)
    if (id === sampleRentalDetail.id) {
      return HttpResponse.json(sampleRentalDetail satisfies RentalDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),

  // Rentals - create
  http.post('/api/rentals', async () => {
    return HttpResponse.json(sampleRentalDetail satisfies RentalDetail, { status: 201 })
  }),

  // Rentals - return
  http.put('/api/rentals/:id/return', ({ params }) => {
    const id = Number(params.id)
    if (id === sampleReturnedRentalDetail.id) {
      return HttpResponse.json(sampleReturnedRentalDetail satisfies RentalDetail)
    }
    return new HttpResponse(null, { status: 404 })
  }),
]
