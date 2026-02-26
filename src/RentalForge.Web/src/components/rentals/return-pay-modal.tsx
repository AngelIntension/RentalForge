import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { returnPaySchema } from '@/lib/validators'

interface ReturnPayModalProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: { amount: number; staffId: number }) => void
  rentalRate: number
  isLoading?: boolean
}

export function ReturnPayModal({ open, onClose, onSubmit, rentalRate, isLoading }: ReturnPayModalProps) {
  const [amount, setAmount] = useState(String(rentalRate))
  const [staffId, setStaffId] = useState('')
  const [errors, setErrors] = useState<Record<string, string>>({})

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})

    const result = returnPaySchema.safeParse({ amount, staffId })
    if (!result.success) {
      const fieldErrors: Record<string, string> = {}
      for (const issue of result.error.issues) {
        const path = issue.path[0]
        if (path && typeof path === 'string') {
          fieldErrors[path] = issue.message
        }
      }
      setErrors(fieldErrors)
      return
    }

    onSubmit(result.data)
  }

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Return & Pay</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="amount">Amount</Label>
            <Input
              id="amount"
              type="number"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
            />
            {errors.amount && <p className="text-sm text-destructive">{errors.amount}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="staffId">Staff ID</Label>
            <Input
              id="staffId"
              type="number"
              value={staffId}
              onChange={(e) => setStaffId(e.target.value)}
            />
            {errors.staffId && <p className="text-sm text-destructive">{errors.staffId}</p>}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Processing...' : 'Return & Pay'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
