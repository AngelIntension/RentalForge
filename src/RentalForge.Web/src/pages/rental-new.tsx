import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { useCreateRental } from '@/hooks/use-rentals'
import { RentalForm } from '@/components/rentals/rental-form'
import { Button } from '@/components/ui/button'
import { ArrowLeft } from 'lucide-react'
import { toast } from 'sonner'
import type { ApiError } from '@/types/api'
import type { CreateRentalRequest } from '@/types/rental'

export function RentalNew() {
  const navigate = useNavigate()
  const createRental = useCreateRental()
  const [apiErrors, setApiErrors] = useState<Record<string, string[]>>()

  const handleSubmit = (data: CreateRentalRequest) => {
    setApiErrors(undefined)
    createRental.mutate(data, {
      onSuccess: () => {
        toast.success('Rental created successfully')
        navigate('/rentals')
      },
      onError: (error) => {
        const apiError = error as unknown as ApiError
        if (apiError.errors) {
          setApiErrors(apiError.errors)
        } else {
          toast.error(apiError.title ?? 'Failed to create rental')
        }
      },
    })
  }

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild>
        <Link to="/rentals">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to rentals
        </Link>
      </Button>

      <div>
        <h1 className="text-3xl font-bold tracking-tight">New Rental</h1>
        <p className="text-muted-foreground">Create a new rental transaction</p>
      </div>

      <div className="max-w-md">
        <RentalForm
          onSubmit={handleSubmit}
          isSubmitting={createRental.isPending}
          apiErrors={apiErrors}
        />
      </div>
    </div>
  )
}
