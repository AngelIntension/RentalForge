import type { FilmListItem, FilmDetail } from '@/types/film'
import type { CustomerListItem } from '@/types/customer'
import type { RentalListItem, RentalDetail, RentalPaymentItem } from '@/types/rental'
import type { PaymentListItem, PaymentDetail } from '@/types/payment'
import type { AuthResponse, UserDto } from '@/types/auth'

// ---------------------------------------------------------------------------
// Films
// ---------------------------------------------------------------------------

export const sampleFilmListItem: FilmListItem = {
  id: 1,
  title: 'Academy Dinosaur',
  description: 'A Epic Drama of a Feminist And a Mad Scientist who must Battle a Teacher in The Canadian Rockies',
  releaseYear: 2006,
  languageId: 1,
  originalLanguageId: null,
  rentalDuration: 6,
  rentalRate: 0.99,
  length: 86,
  replacementCost: 20.99,
  rating: 'PG',
  specialFeatures: ['Deleted Scenes', 'Behind the Scenes'],
  lastUpdate: '2013-05-26T14:50:58.951',
}

export const sampleFilmListItems: FilmListItem[] = [
  sampleFilmListItem,
  {
    id: 2,
    title: 'Ace Goldfinger',
    description: 'A Astounding Epistle of a Database Administrator And a Explorer who must Find a Car in Ancient China',
    releaseYear: 2006,
    languageId: 1,
    originalLanguageId: null,
    rentalDuration: 3,
    rentalRate: 4.99,
    length: 48,
    replacementCost: 12.99,
    rating: 'G',
    specialFeatures: ['Trailers', 'Deleted Scenes'],
    lastUpdate: '2013-05-26T14:50:58.951',
  },
  {
    id: 3,
    title: 'Adaptation Holes',
    description: 'A Astounding Reflection of a Lumberjack And a Car who must Sink a Lumberjack in A Baloon Factory',
    releaseYear: 2006,
    languageId: 1,
    originalLanguageId: null,
    rentalDuration: 7,
    rentalRate: 2.99,
    length: 50,
    replacementCost: 18.99,
    rating: 'NC-17',
    specialFeatures: ['Trailers', 'Deleted Scenes'],
    lastUpdate: '2013-05-26T14:50:58.951',
  },
]

export const sampleFilmDetail: FilmDetail = {
  ...sampleFilmListItem,
  languageName: 'English',
  originalLanguageName: null,
  actors: ['Penelope Guiness', 'Christian Gable', 'Lucille Tracy', 'Sandra Peck', 'Johnny Cage'],
  categories: ['Documentary'],
}

// ---------------------------------------------------------------------------
// Customers
// ---------------------------------------------------------------------------

export const sampleCustomerListItem: CustomerListItem = {
  id: 1,
  storeId: 1,
  firstName: 'Mary',
  lastName: 'Smith',
  email: 'mary.smith@sakilacustomer.org',
  addressId: 5,
  isActive: true,
  createDate: '2006-02-14',
  lastUpdate: '2013-05-26T14:49:45.738',
}

export const sampleCustomerListItems: CustomerListItem[] = [
  sampleCustomerListItem,
  {
    id: 2,
    storeId: 1,
    firstName: 'Patricia',
    lastName: 'Johnson',
    email: 'patricia.johnson@sakilacustomer.org',
    addressId: 6,
    isActive: true,
    createDate: '2006-02-14',
    lastUpdate: '2013-05-26T14:49:45.738',
  },
  {
    id: 3,
    storeId: 2,
    firstName: 'Linda',
    lastName: 'Williams',
    email: 'linda.williams@sakilacustomer.org',
    addressId: 7,
    isActive: true,
    createDate: '2006-02-14',
    lastUpdate: '2013-05-26T14:49:45.738',
  },
]

// ---------------------------------------------------------------------------
// Rentals
// ---------------------------------------------------------------------------

export const sampleRentalListItem: RentalListItem = {
  id: 1,
  rentalDate: '2005-05-24T22:53:30',
  returnDate: null,
  inventoryId: 367,
  customerId: 130,
  staffId: 1,
  lastUpdate: '2013-05-26T14:49:45.738',
  totalPaid: 0,
  rentalRate: 4.99,
  outstandingBalance: 4.99,
}

export const sampleRentalListItems: RentalListItem[] = [
  sampleRentalListItem,
  {
    id: 2,
    rentalDate: '2005-05-24T22:54:33',
    returnDate: '2005-05-28T19:40:33',
    inventoryId: 1525,
    customerId: 459,
    staffId: 1,
    lastUpdate: '2013-05-26T14:49:45.738',
    totalPaid: 4.99,
    rentalRate: 4.99,
    outstandingBalance: 0,
  },
  {
    id: 3,
    rentalDate: '2005-05-24T23:03:39',
    returnDate: null,
    inventoryId: 1711,
    customerId: 408,
    staffId: 2,
    lastUpdate: '2013-05-26T14:49:45.738',
    totalPaid: 0,
    rentalRate: 2.99,
    outstandingBalance: 2.99,
  },
]

export const sampleRentalPaymentItems: RentalPaymentItem[] = [
  { id: 5001, amount: 4.99, paymentDate: '2026-02-25T14:30:00Z', staffId: 1 },
]

export const sampleRentalDetail: RentalDetail = {
  id: 1,
  rentalDate: '2005-05-24T22:53:30',
  returnDate: null,
  inventoryId: 367,
  filmId: 80,
  filmTitle: 'Blanket Beverly',
  storeId: 1,
  customerId: 130,
  customerFirstName: 'Charlotte',
  customerLastName: 'Hunter',
  staffId: 1,
  staffFirstName: 'Mike',
  staffLastName: 'Hillyer',
  lastUpdate: '2013-05-26T14:49:45.738',
  totalPaid: 4.99,
  rentalRate: 4.99,
  outstandingBalance: 0,
  payments: sampleRentalPaymentItems,
}

export const sampleReturnedRentalDetail: RentalDetail = {
  ...sampleRentalDetail,
  returnDate: '2026-02-23T12:00:00',
  lastUpdate: '2026-02-23T12:00:00',
}

// ---------------------------------------------------------------------------
// Payments
// ---------------------------------------------------------------------------

export const samplePaymentListItem: PaymentListItem = {
  id: 5001,
  rentalId: 1001,
  customerId: 42,
  staffId: 1,
  amount: 4.99,
  paymentDate: '2026-02-25T14:30:00Z',
}

export const samplePaymentListItems: PaymentListItem[] = [
  samplePaymentListItem,
  {
    id: 5002,
    rentalId: 1002,
    customerId: 42,
    staffId: 1,
    amount: 2.99,
    paymentDate: '2026-02-24T10:00:00Z',
  },
  {
    id: 5003,
    rentalId: 1003,
    customerId: 43,
    staffId: 2,
    amount: 0.99,
    paymentDate: '2026-02-23T08:00:00Z',
  },
]

export const samplePaymentDetail: PaymentDetail = {
  id: 5001,
  rentalId: 1001,
  customerId: 42,
  customerFirstName: 'Jane',
  customerLastName: 'Doe',
  staffId: 1,
  staffFirstName: 'Mike',
  staffLastName: 'Hillyer',
  amount: 4.99,
  paymentDate: '2026-02-25T14:30:00Z',
  filmTitle: 'Academy Dinosaur',
}

// ---------------------------------------------------------------------------
// Auth
// ---------------------------------------------------------------------------

function base64url(obj: Record<string, unknown>): string {
  const json = JSON.stringify(obj)
  return btoa(json).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
}

export function createTestJwt(overrides?: Partial<{ sub: string; email: string; role: string; exp: number }>): string {
  const header = { alg: 'HS256', typ: 'JWT' }
  const payload = {
    sub: overrides?.sub ?? 'test-user-id-123',
    email: overrides?.email ?? 'staff@rentalforge.dev',
    role: overrides?.role ?? 'Staff',
    jti: 'test-jti-123',
    iat: Math.floor(Date.now() / 1000),
    exp: overrides?.exp ?? Math.floor(Date.now() / 1000) + 900,
  }
  return `${base64url(header)}.${base64url(payload)}.test-signature`
}

export const sampleUserDto: UserDto = {
  id: 'test-user-id-123',
  email: 'staff@rentalforge.dev',
  role: 'Staff',
  customerId: null,
  staffId: 1,
  createdAt: '2026-02-24T10:00:00Z',
}

export const sampleAuthResponse: AuthResponse = {
  token: createTestJwt(),
  refreshToken: 'valid-refresh-token',
  user: sampleUserDto,
}
