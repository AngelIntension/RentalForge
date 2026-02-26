import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api-client'
import type { PagedResponse } from '@/types/api'
import type { PaymentListItem, PaymentDetail, CreatePaymentRequest, PaymentSearchParams } from '@/types/payment'

export function useInfinitePayments(params: Omit<PaymentSearchParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: ['payments', params] as const,
    queryFn: async ({ pageParam }) => {
      const queryParams: Record<string, string> = { page: String(pageParam), pageSize: '10' }
      if (params.customerId !== undefined) queryParams.customerId = String(params.customerId)
      if (params.staffId !== undefined) queryParams.staffId = String(params.staffId)
      if (params.rentalId !== undefined) queryParams.rentalId = String(params.rentalId)
      return api.get<PagedResponse<PaymentListItem>>('/api/payments', queryParams)
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
  })
}

export function useCreatePayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreatePaymentRequest) =>
      api.post<PaymentDetail>('/api/payments', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] })
    },
  })
}
