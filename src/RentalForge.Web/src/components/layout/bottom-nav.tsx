import { NavLink } from 'react-router'
import { Home, Film, Users, ShoppingBag, DollarSign, User } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/use-auth'
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

export function BottomNav() {
  const { role } = useAuth()

  const visibleItems = navItems.filter(
    (item) => !item.roles || (role && item.roles.includes(role)),
  )

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 border-t bg-background md:hidden">
      <div className="flex items-center justify-around py-2">
        {visibleItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              cn(
                'flex flex-col items-center gap-1 px-3 py-1 text-xs',
                isActive ? 'text-primary' : 'text-muted-foreground',
              )
            }
          >
            <Icon className="h-5 w-5" />
            <span>{label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
