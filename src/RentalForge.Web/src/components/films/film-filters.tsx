import { useEffect, useState } from 'react'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import type { FilmSearchParams, MpaaRating } from '@/types/film'

interface FilmFiltersProps {
  onFilterChange: (params: Omit<FilmSearchParams, 'page' | 'pageSize'>) => void
}

const ratings: MpaaRating[] = ['G', 'PG', 'PG-13', 'R', 'NC-17']

export function FilmFilters({ onFilterChange }: FilmFiltersProps) {
  const [search, setSearch] = useState('')
  const [rating, setRating] = useState<MpaaRating>()
  const [yearFrom, setYearFrom] = useState<string>('')
  const [yearTo, setYearTo] = useState<string>('')

  useEffect(() => {
    const timer = setTimeout(() => {
      onFilterChange({
        search: search || undefined,
        rating,
        yearFrom: yearFrom ? Number(yearFrom) : undefined,
        yearTo: yearTo ? Number(yearTo) : undefined,
      })
    }, 300)

    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, rating, yearFrom, yearTo])

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-center">
      <Input
        placeholder="Search films..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="sm:w-64"
      />
      <Select value={rating ?? 'all'} onValueChange={(v) => setRating(v === 'all' ? undefined : v as MpaaRating)}>
        <SelectTrigger className="sm:w-32">
          <SelectValue placeholder="Rating" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All ratings</SelectItem>
          {ratings.map((r) => (
            <SelectItem key={r} value={r}>{r}</SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Input
        type="number"
        placeholder="Year from"
        value={yearFrom}
        onChange={(e) => setYearFrom(e.target.value)}
        className="sm:w-28"
      />
      <Input
        type="number"
        placeholder="Year to"
        value={yearTo}
        onChange={(e) => setYearTo(e.target.value)}
        className="sm:w-28"
      />
    </div>
  )
}
