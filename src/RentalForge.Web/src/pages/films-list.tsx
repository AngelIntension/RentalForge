import { useState } from 'react'
import { useInfiniteFilms } from '@/hooks/use-films'
import { FilmCard } from '@/components/films/film-card'
import { FilmFilters } from '@/components/films/film-filters'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { EmptyState } from '@/components/shared/empty-state'
import { LoadMore } from '@/components/shared/load-more'
import type { FilmSearchParams } from '@/types/film'

export function FilmsList() {
  const [filters, setFilters] = useState<Omit<FilmSearchParams, 'page' | 'pageSize'>>({})
  const { data, isLoading, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, refetch } =
    useInfiniteFilms(filters)

  const films = data?.pages.flatMap((page) => page.items) ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Films</h1>
        <p className="text-muted-foreground">Browse the film catalog</p>
      </div>

      <FilmFilters onFilterChange={setFilters} />

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={(error as { title?: string })?.title} onRetry={() => refetch()} />}
      {!isLoading && !isError && films.length === 0 && <EmptyState message="No films found" />}

      {films.length > 0 && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {films.map((film) => (
              <FilmCard key={film.id} film={film} />
            ))}
          </div>
          <LoadMore onLoadMore={fetchNextPage} hasMore={!!hasNextPage} isLoading={isFetchingNextPage} />
        </>
      )}
    </div>
  )
}
