export interface RegisterRequest {
  readonly userName: string;
  readonly password: string;
}

export interface RegisterResponse {
  readonly userName: string;
  readonly role: string;
}
