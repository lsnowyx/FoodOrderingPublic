export interface CreateOrderResponse {
  readonly orderId: string;
  readonly status: string;
  readonly isPaid: boolean;
  readonly paymentMethod: string;
  readonly paymentStatus: string;
  readonly totalAmount: number;
  readonly paymentUrl: string | null;
}
