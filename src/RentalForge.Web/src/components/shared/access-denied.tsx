import { Link } from 'react-router'
import { ShieldAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function AccessDenied() {
  return (
    <div className="flex flex-col items-center justify-center gap-4 py-16">
      <ShieldAlert className="h-12 w-12 text-muted-foreground" />
      <h1 className="text-2xl font-semibold">Access Denied</h1>
      <p className="text-muted-foreground">
        You don&apos;t have permission to access this page.
      </p>
      <Button asChild>
        <Link to="/">Go to Home</Link>
      </Button>
    </div>
  )
}
