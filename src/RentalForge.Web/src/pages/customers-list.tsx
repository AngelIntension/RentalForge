import { useState, useEffect } from 'react'
import { useInfiniteCustomers } from '@/hooks/use-customers'
import { CustomerCard } from '@/components/customers/customer-card'
import { Input } from '@/components/ui/input'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { EmptyState } from '@/components/shared/empty-state'
import { LoadMore } from '@/components/shared/load-more'

export function CustomersList() {
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState<string>()

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput || undefined)
    }, 300)
    return () => clearTimeout(timer)
  }, [searchInput])

  const { data, isLoading, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage, refetch } =
    useInfiniteCustomers({ search })

  const customers = data?.pages.flatMap((page) => page.items) ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Customers</h1>
        <p className="text-muted-foreground">Browse and search customers</p>
      </div>

      <Input
        placeholder="Search customers..."
        value={searchInput}
        onChange={(e) => setSearchInput(e.target.value)}
        className="max-w-sm"
      />

      {isLoading && <LoadingState />}
      {isError && <ErrorState message={(error as { title?: string })?.title} onRetry={() => refetch()} />}
      {!isLoading && !isError && customers.length === 0 && <EmptyState message="No customers found" />}

      {customers.length > 0 && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {customers.map((customer) => (
              <CustomerCard key={customer.id} customer={customer} />
            ))}
          </div>
          <LoadMore onLoadMore={fetchNextPage} hasMore={!!hasNextPage} isLoading={isFetchingNextPage} />
        </>
      )}
    </div>
  )
}
