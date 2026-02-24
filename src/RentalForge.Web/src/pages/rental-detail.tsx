import { Link, useParams } from 'react-router'
import { useRental } from '@/hooks/use-rentals'
import { RentalDetail } from '@/components/rentals/rental-detail'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { Button } from '@/components/ui/button'
import { ArrowLeft } from 'lucide-react'

export function RentalDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: rental, isLoading, isError, error, refetch } = useRental(Number(id))

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild>
        <Link to="/rentals">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to rentals
        </Link>
      </Button>

      {isLoading && <LoadingState count={1} />}
      {isError && <ErrorState message={(error as { title?: string })?.title ?? 'Rental not found'} onRetry={() => refetch()} />}
      {rental && <RentalDetail rental={rental} />}
    </div>
  )
}
