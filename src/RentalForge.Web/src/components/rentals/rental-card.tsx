import { Link } from 'react-router'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import type { RentalListItem } from '@/types/rental'

interface RentalCardProps {
  rental: RentalListItem
  onReturn?: (id: number) => void
}

export function RentalCard({ rental, onReturn }: RentalCardProps) {
  const isActive = rental.returnDate === null

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
          </div>
          {isActive && onReturn && (
            <Button
              variant="outline"
              size="sm"
              className="mt-3"
              onClick={(e) => {
                e.preventDefault()
                e.stopPropagation()
                onReturn(rental.id)
              }}
            >
              Return
            </Button>
          )}
        </CardContent>
      </Card>
    </Link>
  )
}
