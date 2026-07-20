export interface GuestPaymentLinkResponse {
  readonly isPaid: boolean;
  readonly paymentUrl: string | null;
  readonly message: string;
}
