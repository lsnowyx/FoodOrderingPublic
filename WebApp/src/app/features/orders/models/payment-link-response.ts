export interface PaymentLinkResponse {
  readonly isPaid: boolean;
  readonly canPayOnlineAgain: boolean;
  readonly paymentUrl: string | null;
  readonly message: string;
  readonly paymentStatus: string;
}
