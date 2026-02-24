import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api-client'
import type { PagedResponse } from '@/types/api'
import type { FilmListItem, FilmDetail, FilmSearchParams } from '@/types/film'

export function useInfiniteFilms(params: Omit<FilmSearchParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: ['films', params] as const,
    queryFn: async ({ pageParam }) => {
      const queryParams: Record<string, string> = { page: String(pageParam), pageSize: '10' }
      if (params.search) queryParams.search = params.search
      if (params.category) queryParams.category = params.category
      if (params.rating) queryParams.rating = params.rating
      if (params.yearFrom !== undefined) queryParams.yearFrom = String(params.yearFrom)
      if (params.yearTo !== undefined) queryParams.yearTo = String(params.yearTo)
      return api.get<PagedResponse<FilmListItem>>('/api/films', queryParams)
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
  })
}

export function useFilm(id: number) {
  return useQuery({
    queryKey: ['films', id] as const,
    queryFn: () => api.get<FilmDetail>(`/api/films/${id}`),
  })
}
