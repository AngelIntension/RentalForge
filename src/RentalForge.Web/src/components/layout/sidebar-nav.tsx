import { NavLink } from 'react-router'
import { Home, Film, Users, ShoppingBag, DollarSign, User, LogOut } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/use-auth'
import { Button } from '@/components/ui/button'
import type { LucideIcon } from 'lucide-react'

interface NavItem {
  to: string
  icon: LucideIcon
  label: string
  roles?: string[]
}

const navItems: NavItem[] = [
  { to: '/', icon: Home, label: 'Home' },
  { to: '/films', icon: Film, label: 'Films' },
  { to: '/customers', icon: Users, label: 'Customers', roles: ['Staff', 'Admin'] },
  { to: '/rentals', icon: ShoppingBag, label: 'Rentals' },
  { to: '/payments', icon: DollarSign, label: 'Payments', roles: ['Staff', 'Admin'] },
  { to: '/profile', icon: User, label: 'Profile' },
]

export function SidebarNav() {
  const { role, logout } = useAuth()

  const visibleItems = navItems.filter(
    (item) => !item.roles || (role && item.roles.includes(role)),
  )

  return (
    <aside className="hidden md:flex md:w-64 md:flex-col md:border-r md:bg-background">
      <div className="flex h-14 items-center border-b px-6">
        <h1 className="text-lg font-semibold">RentalForge</h1>
      </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {visibleItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-accent text-accent-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
              )
            }
          >
            <Icon className="h-4 w-4" />
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="border-t px-3 py-4">
        <Button
          variant="ghost"
          className="w-full justify-start gap-3 text-muted-foreground"
          onClick={() => void logout()}
        >
          <LogOut className="h-4 w-4" />
          Logout
        </Button>
      </div>
    </aside>
  )
}
