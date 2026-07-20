export const MAX_ORDER_LINE_COUNT = 50;
export const MAX_ORDER_TOTAL_QUANTITY = 200;

export interface CreateOrderItemRequest {
  readonly menuItemId: string;
  readonly quantity: number;
}

export interface CreateOrderRequest {
  readonly deliveryAddress: string;
  readonly deliveryLatitude: number | null;
  readonly deliveryLongitude: number | null;
  readonly payOnline: boolean;
  readonly items: CreateOrderItemRequest[];
}
