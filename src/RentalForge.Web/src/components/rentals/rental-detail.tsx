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
        <DetailRow label="Rental Rate" value={`$${rental.rentalRate.toFixed(2)}`} />
        <DetailRow label="Total Paid" value={`$${rental.totalPaid.toFixed(2)}`} />
        <DetailRow
          label="Outstanding Balance"
          value={`$${rental.outstandingBalance.toFixed(2)}`}
        />
      </div>

      <Separator />

      <div className="space-y-3">
        <h2 className="text-xl font-semibold">Payments</h2>
        {rental.payments.length === 0 ? (
          <p className="text-sm text-muted-foreground">No payments recorded</p>
        ) : (
          <div className="space-y-2">
            {rental.payments.map((payment) => (
              <div key={payment.id} className="flex justify-between text-sm">
                <span className="text-muted-foreground">
                  {new Date(payment.paymentDate).toLocaleString()} (Staff #{payment.staffId})
                </span>
                <span className="font-medium">${payment.amount.toFixed(2)}</span>
              </div>
            ))}
            <Separator />
            <div className="flex justify-between text-sm font-semibold">
              <span>Total</span>
              <span>${rental.payments.reduce((sum, p) => sum + p.amount, 0).toFixed(2)}</span>
            </div>
          </div>
        )}
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
