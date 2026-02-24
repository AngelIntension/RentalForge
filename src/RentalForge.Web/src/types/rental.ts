export interface RentalListItem {
  id: number;
  rentalDate: string;
  returnDate: string | null;
  inventoryId: number;
  customerId: number;
  staffId: number;
  lastUpdate: string;
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
}

export interface CreateRentalRequest {
  filmId: number;
  storeId: number;
  customerId: number;
  staffId: number;
}

export interface RentalSearchParams {
  customerId?: number;
  activeOnly?: boolean;
  page?: number;
  pageSize?: number;
}
