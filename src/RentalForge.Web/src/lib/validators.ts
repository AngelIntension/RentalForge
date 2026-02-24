import { z } from 'zod/v4';

export const createRentalSchema = z.object({
  filmId: z.coerce.number().int().positive('Film ID is required'),
  storeId: z.coerce.number().int().positive('Store ID is required'),
  customerId: z.coerce.number().int().positive('Customer ID is required'),
  staffId: z.coerce.number().int().positive('Staff ID is required'),
});

export type CreateRentalFormData = z.infer<typeof createRentalSchema>;
