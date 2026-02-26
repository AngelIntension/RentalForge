export interface UserDto {
  id: string
  email: string
  role: 'Admin' | 'Staff' | 'Customer'
  customerId: number | null
  staffId: number | null
  createdAt: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  user: UserDto
}

export interface RefreshResponse {
  token: string
  refreshToken: string
}

export interface RegisterRequest {
  email: string
  password: string
  role?: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RefreshRequest {
  refreshToken: string
}

export interface LogoutRequest {
  refreshToken: string
}
