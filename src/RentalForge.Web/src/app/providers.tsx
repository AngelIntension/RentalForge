import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { RouterProvider } from 'react-router'
import { createQueryClient } from '@/lib/query-client'
import { ThemeProvider } from '@/hooks/use-theme'
import { AuthProvider } from '@/hooks/use-auth'
import { router } from './routes'

const queryClient = createQueryClient()

export function Providers() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </ThemeProvider>
  )
}
