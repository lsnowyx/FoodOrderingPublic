export interface OrderSummary {
  readonly orderId: string;
  readonly createdAt: string;
  readonly orderStatus: string;
  readonly paymentMethod: string;
  readonly paymentStatus: string;
  readonly isPaid: boolean;
  readonly totalAmount: number;
  readonly itemsCount: number;
  readonly canPayOnlineAgain: boolean;
  readonly paymentMessage: string;
  readonly deliveredAt: string | null;
}
