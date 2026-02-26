import { Link } from 'react-router'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/hooks/use-auth'
import type { RentalListItem } from '@/types/rental'

interface RentalCardProps {
  rental: RentalListItem
  onReturn?: (id: number) => void
  onReturnAndPay?: (id: number) => void
}

export function RentalCard({ rental, onReturn, onReturnAndPay }: RentalCardProps) {
  const isActive = rental.returnDate === null
  const { role } = useAuth()
  const isStaffOrAdmin = role === 'Staff' || role === 'Admin'

  return (
    <Link to={`/rentals/${rental.id}`}>
      <Card className="h-full transition-colors hover:bg-accent">
        <CardHeader className="pb-2">
          <div className="flex items-start justify-between gap-2">
            <CardTitle className="text-base">Rental #{rental.id}</CardTitle>
            <Badge variant={isActive ? 'default' : 'secondary'}>
              {isActive ? 'Active' : 'Returned'}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-1 text-sm text-muted-foreground">
            <p>Rented: {new Date(rental.rentalDate).toLocaleDateString()}</p>
            <p>Customer ID: {rental.customerId}</p>
            <p>Inventory ID: {rental.inventoryId}</p>
            <p>
              Paid: <span className="font-medium text-foreground">${rental.totalPaid.toFixed(2)}</span>
              {' / '}
              <span className="font-medium text-foreground">${rental.rentalRate.toFixed(2)}</span>
            </p>
            <p>
              Balance:{' '}
              <span className={`font-medium ${rental.outstandingBalance <= 0 ? 'text-green-600' : 'text-amber-600'}`}>
                ${rental.outstandingBalance.toFixed(2)}
              </span>
            </p>
          </div>
          {isActive && (
            <div className="mt-3 flex gap-2">
              {onReturn && !isStaffOrAdmin && (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={(e) => {
                    e.preventDefault()
                    e.stopPropagation()
                    onReturn(rental.id)
                  }}
                >
                  Return
                </Button>
              )}
              {isStaffOrAdmin && onReturnAndPay && (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={(e) => {
                    e.preventDefault()
                    e.stopPropagation()
                    onReturnAndPay(rental.id)
                  }}
                >
                  Return & Pay
                </Button>
              )}
              {isStaffOrAdmin && onReturn && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.preventDefault()
                    e.stopPropagation()
                    onReturn(rental.id)
                  }}
                >
                  Return Only
                </Button>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </Link>
  )
}
