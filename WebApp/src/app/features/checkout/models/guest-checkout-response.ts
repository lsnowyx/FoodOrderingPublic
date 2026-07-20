export interface GuestCheckoutResponse {
  readonly orderId: string;
  readonly status: string;
  readonly createdAt: string;
  readonly total: number;
  readonly trackingToken: string;
  readonly paymentUrl: string | null;
}
