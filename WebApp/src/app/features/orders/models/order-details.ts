export interface OrderDetailsItem {
  readonly menuItemId: string;
  readonly menuItemName: string;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly lineTotal: number;
  readonly totalCalories: number;
}

export interface OrderDetailsResponse {
  readonly orderId: string;
  readonly createdAt: string;
  readonly orderStatus: string;
  readonly paymentMethod: string;
  readonly paymentStatus: string;
  readonly isPaid: boolean;
  readonly totalAmount: number;
  readonly paidAt: string | null;
  readonly deliveredAt: string | null;
  readonly updatedAt: string | null;
  readonly deliveryAddress: string | null;
  readonly deliveryLatitude: number | null;
  readonly deliveryLongitude: number | null;
  readonly canPayOnlineAgain: boolean;
  readonly paymentMessage: string;
  readonly items: readonly OrderDetailsItem[];
}
