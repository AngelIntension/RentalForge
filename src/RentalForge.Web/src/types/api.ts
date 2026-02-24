export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiError {
  status: number;
  title: string;
  errors: Record<string, string[]> | null;
}
