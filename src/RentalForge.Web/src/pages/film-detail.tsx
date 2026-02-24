import { Link, useParams } from 'react-router'
import { useFilm } from '@/hooks/use-films'
import { FilmDetail } from '@/components/films/film-detail'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { Button } from '@/components/ui/button'
import { ArrowLeft } from 'lucide-react'

export function FilmDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: film, isLoading, isError, error, refetch } = useFilm(Number(id))

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild>
        <Link to="/films">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to films
        </Link>
      </Button>

      {isLoading && <LoadingState count={1} />}
      {isError && <ErrorState message={(error as { title?: string })?.title ?? 'Film not found'} onRetry={() => refetch()} />}
      {film && <FilmDetail film={film} />}
    </div>
  )
}
