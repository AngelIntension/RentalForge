import { useState } from 'react'
import { Link } from 'react-router'
import { useInfiniteRentals, useReturnRental } from '@/hooks/use-rentals'
import { RentalCard } from '@/components/rentals/rental-card'
import { ReturnPayModal } from '@/components/rentals/return-pay-modal'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { EmptyState } from '@/components/shared/empty-state'
import { LoadMore } from '@/components/shared/load-more'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { toast } from 'sonner'

export function RentalsList() {
  const [activeOnly, setActiveOnly] = useState(false)
  const [returnPayRental, setReturnPayRental] = useState<{ id: number; rentalRate: number } | null>(null)
  const { data, isLoading, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, refetch } =
    useInfiniteRentals({ activeOnly })
  const returnRental = useReturnRental()

  const rentals = data?.pages.flatMap((page) => page.items) ?? []

  const handleReturn = (id: number) => {
    returnRental.mutate({ id }, {
      onSuccess: () => toast.success('Rental returned successfully'),
      onError: () => toast.error('Failed to return rental'),
    })
  }

  const handleReturnAndPay = (id: number) => {
    const rental = rentals.find((r) => r.id === id)
    if (rental) {
      setReturnPayRental({ id, rentalRate: rental.rentalRate })
    }
  }

  const handleReturnPaySubmit = (data: { amount: number; staffId: number }) => {
    if (!returnPayRental) return
    returnRental.mutate(
      { id: returnPayRental.id, request: { amount: data.amount, staffId: data.staffId } },
      {
        onSuccess: () => {
          toast.success('Rental returned and payment recorded')
          setReturnPayRental(null)
        },
        onError: () => toast.error('Failed to return rental with payment'),
      },
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Rentals</h1>
          <p className="text-muted-foreground">View and manage rentals</p>
        </div>
        <Button asChild>
          <Link to="/rentals/new">New Rental</Link>
        </Button>
      </div>

      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="activeOnly"
          checked={activeOnly}
          onChange={(e) => setActiveOnly(e.target.checked)}
          className="h-4 w-4"
        />
        <Label htmlFor="activeOnly">Active only</Label>
      </div>

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={(error as { title?: string })?.title} onRetry={() => refetch()} />}
      {!isLoading && !isError && rentals.length === 0 && <EmptyState message="No rentals found" />}

      {rentals.length > 0 && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {rentals.map((rental) => (
              <RentalCard
                key={rental.id}
                rental={rental}
                onReturn={handleReturn}
                onReturnAndPay={handleReturnAndPay}
              />
            ))}
          </div>
          <LoadMore onLoadMore={fetchNextPage} hasMore={!!hasNextPage} isLoading={isFetchingNextPage} />
        </>
      )}

      {returnPayRental && (
        <ReturnPayModal
          open={true}
          onClose={() => setReturnPayRental(null)}
          onSubmit={handleReturnPaySubmit}
          rentalRate={returnPayRental.rentalRate}
          isLoading={returnRental.isPending}
        />
      )}
    </div>
  )
}
