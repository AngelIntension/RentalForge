export interface CustomerListItem {
  id: number;
  storeId: number;
  firstName: string;
  lastName: string;
  email: string | null;
  addressId: number;
  isActive: boolean;
  createDate: string;
  lastUpdate: string;
}

export interface CustomerSearchParams {
  search?: string;
  page?: number;
  pageSize?: number;
}
