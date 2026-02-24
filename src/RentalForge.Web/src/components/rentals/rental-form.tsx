import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { createRentalSchema } from '@/lib/validators'
import type { CreateRentalRequest } from '@/types/rental'

interface RentalFormProps {
  onSubmit: (data: CreateRentalRequest) => void
  isSubmitting?: boolean
  apiErrors?: Record<string, string[]>
}

export function RentalForm({ onSubmit, isSubmitting, apiErrors }: RentalFormProps) {
  const [formData, setFormData] = useState({
    filmId: '',
    storeId: '',
    customerId: '',
    staffId: '',
  })
  const [errors, setErrors] = useState<Record<string, string[]>>({})

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    const result = createRentalSchema.safeParse(formData)

    if (!result.success) {
      const fieldErrors: Record<string, string[]> = {}
      for (const issue of result.error.issues) {
        const field = String(issue.path[0])
        if (!fieldErrors[field]) fieldErrors[field] = []
        fieldErrors[field].push(issue.message)
      }
      setErrors(fieldErrors)
      return
    }

    setErrors({})
    onSubmit(result.data)
  }

  const allErrors = { ...errors, ...apiErrors }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <FormField
        id="filmId"
        label="Film ID"
        value={formData.filmId}
        onChange={(v) => setFormData((prev) => ({ ...prev, filmId: v }))}
        errors={allErrors.filmId}
      />
      <FormField
        id="storeId"
        label="Store ID"
        value={formData.storeId}
        onChange={(v) => setFormData((prev) => ({ ...prev, storeId: v }))}
        errors={allErrors.storeId}
      />
      <FormField
        id="customerId"
        label="Customer ID"
        value={formData.customerId}
        onChange={(v) => setFormData((prev) => ({ ...prev, customerId: v }))}
        errors={allErrors.customerId}
      />
      <FormField
        id="staffId"
        label="Staff ID"
        value={formData.staffId}
        onChange={(v) => setFormData((prev) => ({ ...prev, staffId: v }))}
        errors={allErrors.staffId}
      />
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Creating...' : 'Create Rental'}
      </Button>
    </form>
  )
}

function FormField({
  id,
  label,
  value,
  onChange,
  errors,
}: {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  errors?: string[]
}) {
  return (
    <div className="space-y-1">
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        type="number"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
      {errors?.map((error) => (
        <p key={error} className="text-sm text-destructive">
          {error}
        </p>
      ))}
    </div>
  )
}
