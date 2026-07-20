export const MAX_GUEST_CHECKOUT_LINE_COUNT = 50;
export const MAX_GUEST_CHECKOUT_TOTAL_QUANTITY = 200;

export interface GuestCheckoutItemRequest {
  readonly menuItemId: string;
  readonly quantity: number;
}

export interface GuestCheckoutRequest {
  readonly name: string;
  readonly email: string;
  readonly phoneNumber: string;
  readonly deliveryAddress: string;
  readonly deliveryLatitude: number | null;
  readonly deliveryLongitude: number | null;
  readonly payOnline: boolean;
  readonly items: GuestCheckoutItemRequest[];
}
