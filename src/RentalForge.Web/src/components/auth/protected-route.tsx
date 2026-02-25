import { Navigate } from 'react-router'
import { useAuth } from '@/hooks/use-auth'
import { AccessDenied } from '@/components/shared/access-denied'

interface ProtectedRouteProps {
  children: React.ReactNode
  allowedRoles?: string[]
}

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, role } = useAuth()

  if (isLoading) {
    return null
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && role && !allowedRoles.includes(role)) {
    return <AccessDenied />
  }

  return <>{children}</>
}
