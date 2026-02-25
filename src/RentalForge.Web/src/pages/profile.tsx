import { useAuth } from '@/hooks/use-auth'
import { useCustomer } from '@/hooks/use-customers'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { User, Mail, Shield, Calendar, Store, MapPin } from 'lucide-react'

export function Profile() {
  const { user } = useAuth()

  if (!user) return null

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold tracking-tight">Profile</h1>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="h-5 w-5" />
            Account Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center gap-2">
            <Mail className="h-4 w-4 text-muted-foreground" />
            <span>{user.email}</span>
          </div>
          <div className="flex items-center gap-2">
            <Shield className="h-4 w-4 text-muted-foreground" />
            <Badge variant="secondary">{user.role}</Badge>
          </div>
          {user.createdAt && (
            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4 text-muted-foreground" />
              <span>{new Date(user.createdAt).toLocaleDateString()}</span>
            </div>
          )}
        </CardContent>
      </Card>

      {user.role === 'Customer' && (
        <CustomerDetails customerId={user.customerId} />
      )}
    </div>
  )
}

function CustomerDetails({ customerId }: { customerId: number | null }) {
  if (!customerId) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Customer Details</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">
            No linked customer record. Contact an administrator to link your account.
          </p>
        </CardContent>
      </Card>
    )
  }

  return <LinkedCustomer customerId={customerId} />
}

function LinkedCustomer({ customerId }: { customerId: number }) {
  const { data: customer, isLoading } = useCustomer(customerId)

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Customer Details</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">Loading customer data...</p>
        </CardContent>
      </Card>
    )
  }

  if (!customer) return null

  return (
    <Card>
      <CardHeader>
        <CardTitle>Customer Details</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center gap-2">
          <User className="h-4 w-4 text-muted-foreground" />
          <span>{customer.firstName}</span>
          <span>{customer.lastName}</span>
        </div>
        <div className="flex items-center gap-2">
          <Store className="h-4 w-4 text-muted-foreground" />
          <span>Store #{customer.storeId}</span>
        </div>
        <div className="flex items-center gap-2">
          <MapPin className="h-4 w-4 text-muted-foreground" />
          <span>Address #{customer.addressId}</span>
        </div>
      </CardContent>
    </Card>
  )
}
