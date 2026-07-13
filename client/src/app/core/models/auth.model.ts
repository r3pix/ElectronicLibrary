export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

// Shape of MapIdentityApi's built-in /login and /refresh responses.
export interface AccessTokenResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface CurrentUserModel {
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}
