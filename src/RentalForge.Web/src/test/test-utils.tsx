import { render, type RenderOptions } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'
import type { ReactElement } from 'react'
import { createTestQueryClient } from '@/lib/query-client'
import { AuthProvider } from '@/hooks/use-auth'

interface WrapperOptions {
  initialEntries?: string[]
}

function createWrapper({ initialEntries = ['/'] }: WrapperOptions = {}) {
  const queryClient = createTestQueryClient()

  return function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <MemoryRouter initialEntries={initialEntries}>{children}</MemoryRouter>
        </AuthProvider>
      </QueryClientProvider>
    )
  }
}

function customRender(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'> & WrapperOptions,
) {
  const { initialEntries, ...renderOptions } = options ?? {}
  return render(ui, {
    wrapper: createWrapper({ initialEntries }),
    ...renderOptions,
  })
}

export { customRender as render }
export { screen, waitFor, within, act } from '@testing-library/react'
export { default as userEvent } from '@testing-library/user-event'
