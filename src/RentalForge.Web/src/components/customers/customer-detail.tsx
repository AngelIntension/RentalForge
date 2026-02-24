import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import type { CustomerListItem } from '@/types/customer'

interface CustomerDetailProps {
  customer: CustomerListItem
}

export function CustomerDetail({ customer }: CustomerDetailProps) {
  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold tracking-tight">
            {customer.firstName} {customer.lastName}
          </h1>
          <Badge variant={customer.isActive ? 'default' : 'secondary'}>
            {customer.isActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
        {customer.email && (
          <p className="mt-1 text-muted-foreground">{customer.email}</p>
        )}
      </div>

      <Separator />

      <div className="space-y-2">
        <DetailRow label="Customer ID" value={String(customer.id)} />
        <DetailRow label="Store ID" value={String(customer.storeId)} />
        <DetailRow label="Address ID" value={String(customer.addressId)} />
        <DetailRow label="Created" value={new Date(customer.createDate).toLocaleDateString()} />
        <DetailRow label="Last Updated" value={new Date(customer.lastUpdate).toLocaleString()} />
      </div>
    </div>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  )
}
