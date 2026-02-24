import { describe, it, expect } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { createTestQueryClient } from '@/lib/query-client'
import {
  useInfiniteRentals,
  useRental,
  useCreateRental,
  useReturnRental,
} from '@/hooks/use-rentals'
import { sampleRentalListItems, sampleRentalDetail, sampleReturnedRentalDetail } from '@/test/fixtures/data'
import type { ReactNode } from 'react'

function createWrapper() {
  const queryClient = createTestQueryClient()
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useInfiniteRentals', () => {
  it('returns first page of rentals', async () => {
    const { result } = renderHook(() => useInfiniteRentals({}), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.pages[0].items).toEqual(sampleRentalListItems)
  })

  it('applies customerId filter', async () => {
    const { result } = renderHook(
      () => useInfiniteRentals({ customerId: 130 }),
      { wrapper: createWrapper() },
    )
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.pages[0].items).toBeDefined()
  })

  it('applies activeOnly filter', async () => {
    const { result } = renderHook(
      () => useInfiniteRentals({ activeOnly: true }),
      { wrapper: createWrapper() },
    )
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
  })

  it('fetches next page', async () => {
    const { result } = renderHook(() => useInfiniteRentals({}), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.hasNextPage).toBe(false)
  })
})

describe('useRental', () => {
  it('returns rental detail by ID', async () => {
    const { result } = renderHook(() => useRental(1), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data).toMatchObject({
      id: sampleRentalDetail.id,
      filmTitle: sampleRentalDetail.filmTitle,
      customerFirstName: sampleRentalDetail.customerFirstName,
    })
  })

  it('handles not found', async () => {
    const { result } = renderHook(() => useRental(9999), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.error).toMatchObject({ status: 404 })
  })
})

describe('useCreateRental', () => {
  it('creates a rental', async () => {
    const { result } = renderHook(() => useCreateRental(), { wrapper: createWrapper() })

    await act(async () => {
      result.current.mutate({ filmId: 80, storeId: 1, customerId: 130, staffId: 1 })
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data).toMatchObject({ id: sampleRentalDetail.id })
  })
})

describe('useReturnRental', () => {
  it('returns a rental', async () => {
    const { result } = renderHook(() => useReturnRental(), { wrapper: createWrapper() })

    await act(async () => {
      result.current.mutate(1)
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.returnDate).toBe(sampleReturnedRentalDetail.returnDate)
  })
})
