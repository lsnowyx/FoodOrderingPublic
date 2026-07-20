export interface AuthSession {
  readonly token: string;
  readonly role: string;
  readonly userId: string;
  readonly expiresAt: number;
}
