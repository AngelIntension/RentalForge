import { Outlet } from 'react-router'
import { BottomNav } from '@/components/layout/bottom-nav'
import { SidebarNav } from '@/components/layout/sidebar-nav'
import { ThemeToggle } from '@/components/layout/theme-toggle'
import { Toaster } from '@/components/ui/sonner'

export function RootLayout() {
  return (
    <div className="min-h-screen bg-background">
      <div className="flex">
        <SidebarNav />
        <div className="flex-1">
          <header className="flex h-14 items-center justify-between border-b px-4 md:justify-end">
            <h1 className="text-lg font-semibold md:hidden">RentalForge</h1>
            <ThemeToggle />
          </header>
          <main className="pb-16 md:pb-0">
            <div className="container mx-auto px-4 py-6">
              <Outlet />
            </div>
          </main>
        </div>
      </div>
      <BottomNav />
      <Toaster />
    </div>
  )
}
