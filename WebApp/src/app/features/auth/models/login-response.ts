export interface LoginResponse {
  readonly jwt: string;
  readonly role: string;
  readonly userId: string;
}
