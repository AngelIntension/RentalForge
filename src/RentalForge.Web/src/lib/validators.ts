import { z } from 'zod/v4';

export const createRentalSchema = z.object({
  filmId: z.coerce.number().int().positive('Film ID is required'),
  storeId: z.coerce.number().int().positive('Store ID is required'),
  customerId: z.coerce.number().int().positive('Customer ID is required'),
  staffId: z.coerce.number().int().positive('Staff ID is required'),
});

export type CreateRentalFormData = z.infer<typeof createRentalSchema>;

export const createPaymentSchema = z.object({
  rentalId: z.coerce.number().int().positive('Rental ID is required'),
  amount: z.coerce.number().positive('Amount must be greater than 0'),
  staffId: z.coerce.number().int().positive('Staff ID is required'),
});

export type CreatePaymentFormData = z.infer<typeof createPaymentSchema>;

export const returnPaySchema = z.object({
  amount: z.coerce.number().positive('Amount must be greater than 0'),
  staffId: z.coerce.number().int().positive('Staff ID is required'),
});

export type ReturnPayFormData = z.infer<typeof returnPaySchema>;

const passwordRegex = {
  uppercase: /[A-Z]/,
  lowercase: /[a-z]/,
  digit: /\d/,
  special: /[^a-zA-Z0-9]/,
};

export const registerSchema = z.object({
  email: z.email('Please enter a valid email address'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .refine((v) => passwordRegex.uppercase.test(v), 'Password must contain at least one uppercase letter')
    .refine((v) => passwordRegex.lowercase.test(v), 'Password must contain at least one lowercase letter')
    .refine((v) => passwordRegex.digit.test(v), 'Password must contain at least one digit')
    .refine((v) => passwordRegex.special.test(v), 'Password must contain at least one special character'),
  confirmPassword: z.string(),
}).check((ctx) => {
  if (ctx.value.password !== ctx.value.confirmPassword) {
    ctx.issues.push({
      code: 'custom',
      input: ctx.value.confirmPassword,
      message: 'Passwords do not match',
      path: ['confirmPassword'],
    });
  }
});

export type RegisterFormData = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.email('Please enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
});

export type LoginFormData = z.infer<typeof loginSchema>;
