export const MAX_CART_ITEM_QUANTITY = 99;

// Price and calorie values are display snapshots. Checkout must revalidate them with the backend.
export interface CartItem {
  readonly menuItemId: string;
  readonly name: string;
  readonly imageUrl: string | null;
  readonly unitPrice: number;
  readonly caloriesPerUnit: number;
  readonly quantity: number;
}
