import { createBrowserRouter } from 'react-router'
import { RootLayout } from './root-layout'
import { Home } from '@/pages/home'
import { FilmsList } from '@/pages/films-list'
import { FilmDetailPage } from '@/pages/film-detail'
import { CustomersList } from '@/pages/customers-list'
import { CustomerDetailPage } from '@/pages/customer-detail'
import { RentalsList } from '@/pages/rentals-list'
import { RentalNew } from '@/pages/rental-new'
import { RentalDetailPage } from '@/pages/rental-detail'
import { PaymentsList } from '@/pages/payments-list'
import { Profile } from '@/pages/profile'
import { NotFound } from '@/pages/not-found'
import { Login } from '@/pages/login'
import { Register } from '@/pages/register'
import { ProtectedRoute } from '@/components/auth/protected-route'

export const router = createBrowserRouter([
  // Public auth routes (no nav/layout)
  { path: 'login', element: <Login /> },
  { path: 'register', element: <Register /> },
  // App routes (with layout, protected)
  {
    element: (
      <ProtectedRoute>
        <RootLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Home /> },
      { path: 'films', element: <FilmsList /> },
      { path: 'films/:id', element: <FilmDetailPage /> },
      {
        path: 'customers',
        element: (
          <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
            <CustomersList />
          </ProtectedRoute>
        ),
      },
      {
        path: 'customers/:id',
        element: (
          <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
            <CustomerDetailPage />
          </ProtectedRoute>
        ),
      },
      { path: 'rentals', element: <RentalsList /> },
      {
        path: 'rentals/new',
        element: (
          <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
            <RentalNew />
          </ProtectedRoute>
        ),
      },
      { path: 'rentals/:id', element: <RentalDetailPage /> },
      {
        path: 'payments',
        element: (
          <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
            <PaymentsList />
          </ProtectedRoute>
        ),
      },
      { path: 'profile', element: <Profile /> },
      { path: '*', element: <NotFound /> },
    ],
  },
])
