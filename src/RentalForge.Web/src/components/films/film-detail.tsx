import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import type { FilmDetail as FilmDetailType } from '@/types/film'

interface FilmDetailProps {
  film: FilmDetailType
}

export function FilmDetail({ film }: FilmDetailProps) {
  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold tracking-tight">{film.title}</h1>
          {film.rating && <Badge variant="secondary" className="text-sm">{film.rating}</Badge>}
        </div>
        {film.description && (
          <p className="mt-2 text-muted-foreground">{film.description}</p>
        )}
      </div>

      <Separator />

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <DetailRow label="Release Year" value={film.releaseYear?.toString()} />
          <DetailRow label="Language" value={film.languageName} />
          {film.originalLanguageName && (
            <DetailRow label="Original Language" value={film.originalLanguageName} />
          )}
          <DetailRow label="Length" value={film.length ? `${film.length} min` : undefined} />
        </div>
        <div className="space-y-2">
          <DetailRow label="Rental Rate" value={`$${film.rentalRate.toFixed(2)}/day`} />
          <DetailRow label="Rental Duration" value={`${film.rentalDuration} days`} />
          <DetailRow label="Replacement Cost" value={`$${film.replacementCost.toFixed(2)}`} />
        </div>
      </div>

      {film.specialFeatures && film.specialFeatures.length > 0 && (
        <>
          <Separator />
          <div>
            <h2 className="mb-2 font-semibold">Special Features</h2>
            <div className="flex flex-wrap gap-2">
              {film.specialFeatures.map((feature) => (
                <Badge key={feature} variant="outline">{feature}</Badge>
              ))}
            </div>
          </div>
        </>
      )}

      {film.categories.length > 0 && (
        <>
          <Separator />
          <div>
            <h2 className="mb-2 font-semibold">Categories</h2>
            <div className="flex flex-wrap gap-2">
              {film.categories.map((cat) => (
                <Badge key={cat}>{cat}</Badge>
              ))}
            </div>
          </div>
        </>
      )}

      {film.actors.length > 0 && (
        <>
          <Separator />
          <div>
            <h2 className="mb-2 font-semibold">Actors</h2>
            <p className="text-muted-foreground">{film.actors.join(', ')}</p>
          </div>
        </>
      )}
    </div>
  )
}

function DetailRow({ label, value }: { label: string; value?: string }) {
  if (!value) return null
  return (
    <div className="flex justify-between">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  )
}
