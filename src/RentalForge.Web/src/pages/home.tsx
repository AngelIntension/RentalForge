import { Link } from 'react-router'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Film, Users, ShoppingBag } from 'lucide-react'

const quickLinks = [
  { to: '/films', icon: Film, title: 'Films', description: 'Browse the film catalog' },
  { to: '/customers', icon: Users, title: 'Customers', description: 'Manage customer accounts' },
  { to: '/rentals', icon: ShoppingBag, title: 'Rentals', description: 'View and manage rentals' },
]

export function Home() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Welcome to RentalForge</h1>
        <p className="text-muted-foreground">DVD rental management made simple</p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {quickLinks.map(({ to, icon: Icon, title, description }) => (
          <Link key={to} to={to}>
            <Card className="transition-colors hover:bg-accent">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <Icon className="h-8 w-8 text-primary" />
                  <div>
                    <CardTitle>{title}</CardTitle>
                    <CardDescription>{description}</CardDescription>
                  </div>
                </div>
              </CardHeader>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  )
}
