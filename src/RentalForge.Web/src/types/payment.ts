export interface PaymentListItem {
  id: number;
  rentalId: number;
  customerId: number;
  staffId: number;
  amount: number;
  paymentDate: string;
}

export interface PaymentDetail {
  id: number;
  rentalId: number;
  customerId: number;
  customerFirstName: string;
  customerLastName: string;
  staffId: number;
  staffFirstName: string;
  staffLastName: string;
  amount: number;
  paymentDate: string;
  filmTitle: string;
}

export interface CreatePaymentRequest {
  rentalId: number;
  amount: number;
  paymentDate?: string;
  staffId: number;
}

export interface PaymentSearchParams {
  customerId?: number;
  staffId?: number;
  rentalId?: number;
  page?: number;
  pageSize?: number;
}
