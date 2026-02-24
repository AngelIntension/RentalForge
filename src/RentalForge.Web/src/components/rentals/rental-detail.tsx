import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import type { RentalDetail as RentalDetailType } from '@/types/rental'

interface RentalDetailProps {
  rental: RentalDetailType
}

export function RentalDetail({ rental }: RentalDetailProps) {
  const isActive = rental.returnDate === null

  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold tracking-tight">Rental #{rental.id}</h1>
          <Badge variant={isActive ? 'default' : 'secondary'}>
            {isActive ? 'Active' : 'Returned'}
          </Badge>
        </div>
      </div>

      <Separator />

      <div className="space-y-2">
        <DetailRow label="Film" value={rental.filmTitle} />
        <DetailRow label="Customer" value={`${rental.customerFirstName} ${rental.customerLastName}`} />
        <DetailRow label="Staff" value={`${rental.staffFirstName} ${rental.staffLastName}`} />
        <DetailRow label="Store ID" value={String(rental.storeId)} />
        <DetailRow label="Inventory ID" value={String(rental.inventoryId)} />
        <DetailRow label="Rental Date" value={new Date(rental.rentalDate).toLocaleString()} />
        <DetailRow
          label="Return Date"
          value={rental.returnDate ? new Date(rental.returnDate).toLocaleString() : 'Not returned'}
        />
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
