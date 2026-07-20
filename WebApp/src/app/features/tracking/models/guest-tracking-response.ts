export interface GuestTrackingItem {
  readonly menuItemId: string;
  readonly menuItemName: string;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly lineTotal: number;
  readonly totalCalories: number;
}

export interface GuestTrackingResponse {
  readonly orderId: string;
  readonly status: string;
  readonly deliveryStatus: string;
  readonly paymentMethod: string;
  readonly paymentStatus: string;
  readonly isPaid: boolean;
  readonly canPayOnlineAgain: boolean;
  readonly paymentMessage: string;
  readonly updatedAt: string | null;
  readonly deliveredAt: string | null;
  readonly items: readonly GuestTrackingItem[];
}
