import { Button } from '@/components/ui/button'

interface LoadMoreProps {
  onLoadMore: () => void
  hasMore: boolean
  isLoading?: boolean
}

export function LoadMore({ onLoadMore, hasMore, isLoading }: LoadMoreProps) {
  if (!hasMore) return null

  return (
    <div className="flex justify-center py-4">
      <Button variant="outline" onClick={onLoadMore} disabled={isLoading}>
        {isLoading ? 'Loading...' : 'Load more'}
      </Button>
    </div>
  )
}
