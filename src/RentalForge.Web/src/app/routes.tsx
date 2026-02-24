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
import { Profile } from '@/pages/profile'
import { NotFound } from '@/pages/not-found'

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      { index: true, element: <Home /> },
      { path: 'films', element: <FilmsList /> },
      { path: 'films/:id', element: <FilmDetailPage /> },
      { path: 'customers', element: <CustomersList /> },
      { path: 'customers/:id', element: <CustomerDetailPage /> },
      { path: 'rentals', element: <RentalsList /> },
      { path: 'rentals/new', element: <RentalNew /> },
      { path: 'rentals/:id', element: <RentalDetailPage /> },
      { path: 'profile', element: <Profile /> },
      { path: '*', element: <NotFound /> },
    ],
  },
])
