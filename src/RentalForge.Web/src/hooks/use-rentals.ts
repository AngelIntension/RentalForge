import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api-client'
import type { PagedResponse } from '@/types/api'
import type { RentalListItem, RentalDetail, CreateRentalRequest, RentalSearchParams, ReturnRentalRequest } from '@/types/rental'

export function useInfiniteRentals(params: Omit<RentalSearchParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: ['rentals', params] as const,
    queryFn: async ({ pageParam }) => {
      const queryParams: Record<string, string> = { page: String(pageParam), pageSize: '10' }
      if (params.customerId !== undefined) queryParams.customerId = String(params.customerId)
      if (params.activeOnly) queryParams.activeOnly = 'true'
      return api.get<PagedResponse<RentalListItem>>('/api/rentals', queryParams)
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
  })
}

export function useRental(id: number) {
  return useQuery({
    queryKey: ['rentals', id] as const,
    queryFn: () => api.get<RentalDetail>(`/api/rentals/${id}`),
  })
}

export function useCreateRental() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateRentalRequest) =>
      api.post<RentalDetail>('/api/rentals', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rentals'] })
    },
  })
}

export function useReturnRental() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, request }: { id: number; request?: ReturnRentalRequest }) =>
      api.put<RentalDetail>(`/api/rentals/${id}/return`, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rentals'] })
    },
  })
}
