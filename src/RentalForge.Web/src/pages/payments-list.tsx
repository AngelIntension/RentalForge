import { useInfinitePayments } from '@/hooks/use-payments'
import { PaymentCard } from '@/components/payments/payment-card'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { EmptyState } from '@/components/shared/empty-state'
import { LoadMore } from '@/components/shared/load-more'

export function PaymentsList() {
  const { data, isLoading, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, refetch } =
    useInfinitePayments({})

  const payments = data?.pages.flatMap((page) => page.items) ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Payments</h1>
        <p className="text-muted-foreground">View payment history</p>
      </div>

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={(error as { title?: string })?.title} onRetry={() => refetch()} />}
      {!isLoading && !isError && payments.length === 0 && <EmptyState message="No payments found" />}

      {payments.length > 0 && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {payments.map((payment) => (
              <PaymentCard key={payment.id} payment={payment} />
            ))}
          </div>
          <LoadMore onLoadMore={fetchNextPage} hasMore={!!hasNextPage} isLoading={isFetchingNextPage} />
        </>
      )}
    </div>
  )
}
