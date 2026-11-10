export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

export interface StoredSession extends LoginResponse {
  name: string;
  role: string;
}
