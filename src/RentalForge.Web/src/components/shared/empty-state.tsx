interface EmptyStateProps {
  message?: string
  icon?: React.ReactNode
}

export function EmptyState({ message = 'No results found', icon }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-4 py-12">
      {icon && <div className="text-muted-foreground">{icon}</div>}
      <p className="text-muted-foreground text-lg">{message}</p>
    </div>
  )
}
