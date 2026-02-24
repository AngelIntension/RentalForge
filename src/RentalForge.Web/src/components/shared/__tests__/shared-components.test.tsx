import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test/test-utils'
import { ErrorState } from '@/components/shared/error-state'
import { EmptyState } from '@/components/shared/empty-state'
import { LoadingState } from '@/components/shared/loading-state'
import { LoadMore } from '@/components/shared/load-more'

describe('ErrorState', () => {
  it('renders default error message', () => {
    render(<ErrorState />)
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
  })

  it('renders custom error message', () => {
    render(<ErrorState message="Custom error" />)
    expect(screen.getByText('Custom error')).toBeInTheDocument()
  })

  it('renders retry button when onRetry provided', () => {
    const onRetry = vi.fn()
    render(<ErrorState onRetry={onRetry} />)
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument()
  })

  it('does not render retry button when onRetry not provided', () => {
    render(<ErrorState />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })
})

describe('EmptyState', () => {
  it('renders default empty message', () => {
    render(<EmptyState />)
    expect(screen.getByText('No results found')).toBeInTheDocument()
  })

  it('renders custom message', () => {
    render(<EmptyState message="No films available" />)
    expect(screen.getByText('No films available')).toBeInTheDocument()
  })
})

describe('LoadingState', () => {
  it('renders skeleton placeholders', () => {
    const { container } = render(<LoadingState count={3} />)
    // Each loading item has 3 skeletons
    const skeletons = container.querySelectorAll('[data-slot="skeleton"]')
    expect(skeletons.length).toBe(9)
  })
})

describe('LoadMore', () => {
  it('renders load more button when hasMore is true', () => {
    render(<LoadMore onLoadMore={() => {}} hasMore={true} />)
    expect(screen.getByRole('button', { name: /load more/i })).toBeInTheDocument()
  })

  it('does not render when hasMore is false', () => {
    render(<LoadMore onLoadMore={() => {}} hasMore={false} />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('shows loading text when isLoading', () => {
    render(<LoadMore onLoadMore={() => {}} hasMore={true} isLoading={true} />)
    expect(screen.getByRole('button', { name: /loading/i })).toBeInTheDocument()
  })
})
