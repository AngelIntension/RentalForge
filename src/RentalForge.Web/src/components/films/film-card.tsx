import { Link } from 'react-router'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import type { FilmListItem } from '@/types/film'

interface FilmCardProps {
  film: FilmListItem
}

export function FilmCard({ film }: FilmCardProps) {
  return (
    <Link to={`/films/${film.id}`}>
      <Card className="h-full transition-colors hover:bg-accent">
        <CardHeader className="pb-2">
          <div className="flex items-start justify-between gap-2">
            <CardTitle className="text-base leading-tight">{film.title}</CardTitle>
            {film.rating && <Badge variant="secondary">{film.rating}</Badge>}
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            {film.releaseYear && <span>{film.releaseYear}</span>}
            <span>${film.rentalRate.toFixed(2)}/day</span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}
