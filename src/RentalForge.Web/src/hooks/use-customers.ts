import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api-client'
import type { PagedResponse } from '@/types/api'
import type { CustomerListItem, CustomerSearchParams } from '@/types/customer'

export function useInfiniteCustomers(params: Omit<CustomerSearchParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: ['customers', params] as const,
    queryFn: async ({ pageParam }) => {
      const queryParams: Record<string, string> = { page: String(pageParam), pageSize: '10' }
      if (params.search) queryParams.search = params.search
      return api.get<PagedResponse<CustomerListItem>>('/api/customers', queryParams)
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
  })
}

export function useCustomer(id: number) {
  return useQuery({
    queryKey: ['customers', id] as const,
    queryFn: () => api.get<CustomerListItem>(`/api/customers/${id}`),
  })
}
