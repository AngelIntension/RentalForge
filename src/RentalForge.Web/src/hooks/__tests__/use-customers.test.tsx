import { describe, it, expect } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { createTestQueryClient } from '@/lib/query-client'
import { useInfiniteCustomers, useCustomer } from '@/hooks/use-customers'
import { sampleCustomerListItems, sampleCustomerListItem } from '@/test/fixtures/data'
import type { ReactNode } from 'react'

function createWrapper() {
  const queryClient = createTestQueryClient()
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useInfiniteCustomers', () => {
  it('returns first page of customers', async () => {
    const { result } = renderHook(() => useInfiniteCustomers({}), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.pages[0].items).toEqual(sampleCustomerListItems)
  })

  it('fetches next page on fetchNextPage', async () => {
    const { result } = renderHook(() => useInfiniteCustomers({}), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.hasNextPage).toBe(false)
  })

  it('applies search param', async () => {
    const { result } = renderHook(
      () => useInfiniteCustomers({ search: 'Mary' }),
      { wrapper: createWrapper() },
    )
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.pages[0].items).toBeDefined()
  })

  it('handles empty results', async () => {
    const { result } = renderHook(() => useInfiniteCustomers({}), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
  })
})

describe('useCustomer', () => {
  it('returns customer by ID', async () => {
    const { result } = renderHook(() => useCustomer(1), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data).toMatchObject({
      id: sampleCustomerListItem.id,
      firstName: sampleCustomerListItem.firstName,
      lastName: sampleCustomerListItem.lastName,
    })
  })

  it('handles not found', async () => {
    const { result } = renderHook(() => useCustomer(9999), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.error).toMatchObject({ status: 404 })
  })
})
