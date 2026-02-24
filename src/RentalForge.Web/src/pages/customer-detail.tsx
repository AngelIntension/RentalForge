import { Link, useParams } from 'react-router'
import { useCustomer } from '@/hooks/use-customers'
import { CustomerDetail } from '@/components/customers/customer-detail'
import { LoadingState } from '@/components/shared/loading-state'
import { ErrorState } from '@/components/shared/error-state'
import { Button } from '@/components/ui/button'
import { ArrowLeft } from 'lucide-react'

export function CustomerDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: customer, isLoading, isError, error, refetch } = useCustomer(Number(id))

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild>
        <Link to="/customers">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to customers
        </Link>
      </Button>

      {isLoading && <LoadingState count={1} />}
      {isError && <ErrorState message={(error as { title?: string })?.title ?? 'Customer not found'} onRetry={() => refetch()} />}
      {customer && <CustomerDetail customer={customer} />}
    </div>
  )
}
