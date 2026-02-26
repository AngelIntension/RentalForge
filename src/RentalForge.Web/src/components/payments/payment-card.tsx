import { Link } from 'react-router'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import type { PaymentListItem } from '@/types/payment'

interface PaymentCardProps {
  payment: PaymentListItem
}

export function PaymentCard({ payment }: PaymentCardProps) {
  return (
    <Link to={`/rentals/${payment.rentalId}`}>
      <Card className="h-full transition-colors hover:bg-accent">
        <CardHeader className="pb-2">
          <CardTitle className="text-base">Payment #{payment.id}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-1 text-sm text-muted-foreground">
            <p>Amount: <span className="font-medium text-foreground">${payment.amount.toFixed(2)}</span></p>
            <p>Date: {new Date(payment.paymentDate).toLocaleDateString()}</p>
            <p>Rental ID: {payment.rentalId}</p>
            <p>Customer ID: {payment.customerId}</p>
            <p>Staff ID: {payment.staffId}</p>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}
