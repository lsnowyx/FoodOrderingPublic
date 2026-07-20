import { Injectable, computed, effect, signal } from '@angular/core';

import { CartItem, MAX_CART_ITEM_QUANTITY } from '../models/cart-item';

export const CART_STORAGE_KEY = 'food-ordering-cart-v1';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function getLocalStorage(): Storage | null {
  try {
    return typeof localStorage === 'undefined' ? null : localStorage;
  } catch {
    return null;
  }
}

function parseCartItem(value: unknown): CartItem | null {
  if (typeof value !== 'object' || value === null) {
    return null;
  }

  const item = value as Record<string, unknown>;
  const menuItemId = item['menuItemId'];
  const name = item['name'];
  const imageUrl = item['imageUrl'];
  const unitPrice = item['unitPrice'];
  const caloriesPerUnit = item['caloriesPerUnit'];
  const quantity = item['quantity'];

  if (
    typeof menuItemId !== 'string' ||
    !GUID_PATTERN.test(menuItemId) ||
    typeof name !== 'string' ||
    name.trim().length === 0 ||
    !(imageUrl === null || (typeof imageUrl === 'string' && imageUrl.trim().length > 0)) ||
    typeof unitPrice !== 'number' ||
    !Number.isFinite(unitPrice) ||
    unitPrice < 0 ||
    typeof caloriesPerUnit !== 'number' ||
    !Number.isFinite(caloriesPerUnit) ||
    caloriesPerUnit < 0 ||
    typeof quantity !== 'number' ||
    !Number.isInteger(quantity) ||
    quantity < 1 ||
    quantity > MAX_CART_ITEM_QUANTITY
  ) {
    return null;
  }

  return {
    menuItemId,
    name: name.trim(),
    imageUrl,
    unitPrice,
    caloriesPerUnit,
    quantity
  };
}

function restoreCartItems(storage: Storage | null): CartItem[] {
  if (storage === null) {
    return [];
  }

  try {
    const storedValue = storage.getItem(CART_STORAGE_KEY);
    if (storedValue === null) {
      return [];
    }

    const parsedValue: unknown = JSON.parse(storedValue);
    if (!Array.isArray(parsedValue)) {
      return [];
    }

    const restoredItems: CartItem[] = [];
    const restoredIds = new Set<string>();

    for (const value of parsedValue) {
      const item = parseCartItem(value);
      if (item === null || restoredIds.has(item.menuItemId)) {
        continue;
      }

      restoredItems.push(item);
      restoredIds.add(item.menuItemId);
    }

    return restoredItems;
  } catch {
    return [];
  }
}

function persistCartItems(storage: Storage | null, items: readonly CartItem[]): void {
  if (storage === null) {
    return;
  }

  try {
    storage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
  } catch {
    // Storage can be unavailable or full. The in-memory cart remains usable.
  }
}

@Injectable({ providedIn: 'root' })
export class CartStore {
  private readonly storage = getLocalStorage();
  private readonly writableItems = signal<CartItem[]>(restoreCartItems(this.storage));

  readonly items = this.writableItems.asReadonly();
  readonly totalQuantity = computed(() =>
    this.items().reduce((total, item) => total + item.quantity, 0)
  );
  readonly totalPrice = computed(() =>
    this.items().reduce((total, item) => total + item.unitPrice * item.quantity, 0)
  );
  readonly totalCalories = computed(() =>
    this.items().reduce((total, item) => total + item.caloriesPerUnit * item.quantity, 0)
  );
  readonly distinctItemCount = computed(() => this.items().length);
  readonly isEmpty = computed(() => this.items().length === 0);

  constructor() {
    effect(() => persistCartItems(this.storage, this.items()));
  }

  addItem(item: CartItem): boolean {
    const normalizedItem = parseCartItem({ ...item, quantity: 1 });
    if (normalizedItem === null) {
      return false;
    }

    const existingItem = this.items().find(
      currentItem => currentItem.menuItemId === normalizedItem.menuItemId
    );

    if (existingItem?.quantity === MAX_CART_ITEM_QUANTITY) {
      return false;
    }

    this.writableItems.update(items => {
      if (existingItem === undefined) {
        return [...items, normalizedItem];
      }

      return items.map(currentItem =>
        currentItem.menuItemId === normalizedItem.menuItemId
          ? { ...normalizedItem, quantity: currentItem.quantity + 1 }
          : currentItem
      );
    });

    return true;
  }

  removeItem(menuItemId: string): void {
    this.writableItems.update(items =>
      items.filter(item => item.menuItemId !== menuItemId)
    );
  }

  increaseQuantity(menuItemId: string): boolean {
    const item = this.items().find(currentItem => currentItem.menuItemId === menuItemId);
    if (item === undefined || item.quantity >= MAX_CART_ITEM_QUANTITY) {
      return false;
    }

    this.updateQuantity(menuItemId, item.quantity + 1);
    return true;
  }

  decreaseQuantity(menuItemId: string): void {
    const item = this.items().find(currentItem => currentItem.menuItemId === menuItemId);
    if (item === undefined) {
      return;
    }

    this.updateQuantity(menuItemId, item.quantity - 1);
  }

  updateQuantity(menuItemId: string, quantity: number): void {
    const itemExists = this.items().some(item => item.menuItemId === menuItemId);
    if (!itemExists || !Number.isFinite(quantity) || !Number.isInteger(quantity)) {
      return;
    }

    if (quantity < 1) {
      this.removeItem(menuItemId);
      return;
    }

    const validQuantity = Math.min(quantity, MAX_CART_ITEM_QUANTITY);
    this.writableItems.update(items =>
      items.map(item =>
        item.menuItemId === menuItemId
          ? { ...item, quantity: validQuantity }
          : item
      )
    );
  }

  clear(): void {
    this.writableItems.set([]);
  }
}
