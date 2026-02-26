export interface RentalListItem {
  id: number;
  rentalDate: string;
  returnDate: string | null;
  inventoryId: number;
  customerId: number;
  staffId: number;
  lastUpdate: string;
  totalPaid: number;
  rentalRate: number;
  outstandingBalance: number;
}

export interface RentalPaymentItem {
  id: number;
  amount: number;
  paymentDate: string;
  staffId: number;
}

export interface RentalDetail {
  id: number;
  rentalDate: string;
  returnDate: string | null;
  inventoryId: number;
  filmId: number;
  filmTitle: string;
  storeId: number;
  customerId: number;
  customerFirstName: string;
  customerLastName: string;
  staffId: number;
  staffFirstName: string;
  staffLastName: string;
  lastUpdate: string;
  totalPaid: number;
  rentalRate: number;
  outstandingBalance: number;
  payments: RentalPaymentItem[];
}

export interface CreateRentalRequest {
  filmId: number;
  storeId: number;
  customerId: number;
  staffId: number;
}

export interface ReturnRentalRequest {
  amount?: number;
  staffId?: number;
}

export interface RentalSearchParams {
  customerId?: number;
  activeOnly?: boolean;
  page?: number;
  pageSize?: number;
}
