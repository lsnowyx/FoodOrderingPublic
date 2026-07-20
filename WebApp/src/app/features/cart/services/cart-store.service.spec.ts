import { TestBed } from '@angular/core/testing';

import { CartItem, MAX_CART_ITEM_QUANTITY } from '../models/cart-item';
import { CART_STORAGE_KEY, CartStore } from './cart-store.service';

const PIZZA: CartItem = {
  menuItemId: '11111111-1111-1111-1111-111111111111',
  name: 'Pizza',
  imageUrl: 'https://example.com/pizza.jpg',
  unitPrice: 10.5,
  caloriesPerUnit: 650,
  quantity: 1
};

const SALAD: CartItem = {
  menuItemId: '22222222-2222-2222-2222-222222222222',
  name: 'Salad',
  imageUrl: null,
  unitPrice: 3,
  caloriesPerUnit: 125.5,
  quantity: 1
};

describe('CartStore', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.clear();
  });

  function createStore(): CartStore {
    return TestBed.inject(CartStore);
  }

  it('starts with an empty cart', () => {
    const store = createStore();

    expect(store.items()).toEqual([]);
    expect(store.isEmpty()).toBe(true);
    expect(store.distinctItemCount()).toBe(0);
  });

  it('adds a new item with quantity one', () => {
    const store = createStore();

    expect(store.addItem({ ...PIZZA, quantity: 25 })).toBe(true);
    expect(store.items()).toEqual([PIZZA]);
  });

  it('increments an existing item when it is added again', () => {
    const store = createStore();

    store.addItem(PIZZA);
    store.addItem(PIZZA);

    expect(store.items()[0]?.quantity).toBe(2);
    expect(store.distinctItemCount()).toBe(1);
  });

  it('enforces the maximum item quantity', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.updateQuantity(PIZZA.menuItemId, MAX_CART_ITEM_QUANTITY);

    expect(store.addItem(PIZZA)).toBe(false);
    expect(store.increaseQuantity(PIZZA.menuItemId)).toBe(false);
    expect(store.items()[0]?.quantity).toBe(MAX_CART_ITEM_QUANTITY);
  });

  it('removes an item', () => {
    const store = createStore();
    store.addItem(PIZZA);

    store.removeItem(PIZZA.menuItemId);

    expect(store.items()).toEqual([]);
  });

  it('increases and decreases quantity, removing the item below one', () => {
    const store = createStore();
    store.addItem(PIZZA);

    expect(store.increaseQuantity(PIZZA.menuItemId)).toBe(true);
    expect(store.items()[0]?.quantity).toBe(2);

    store.decreaseQuantity(PIZZA.menuItemId);
    expect(store.items()[0]?.quantity).toBe(1);

    store.decreaseQuantity(PIZZA.menuItemId);
    expect(store.items()).toEqual([]);
  });

  it('does not change the cart when an unknown item is updated', () => {
    const store = createStore();
    store.addItem(PIZZA);

    store.updateQuantity(SALAD.menuItemId, 5);

    expect(store.items()).toEqual([PIZZA]);
  });

  it('clears all items', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.addItem(SALAD);

    store.clear();

    expect(store.items()).toEqual([]);
    expect(store.isEmpty()).toBe(true);
  });

  it('calculates total quantity across all cart lines', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.addItem(PIZZA);
    store.addItem(SALAD);

    expect(store.totalQuantity()).toBe(3);
  });

  it('calculates total price', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.addItem(PIZZA);
    store.addItem(SALAD);

    expect(store.totalPrice()).toBe(24);
  });

  it('calculates total calories', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.addItem(PIZZA);
    store.addItem(SALAD);

    expect(store.totalCalories()).toBe(1425.5);
  });

  it('restores valid localStorage data', () => {
    const storedPizza = { ...PIZZA, quantity: 2 };
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify([storedPizza]));

    const store = createStore();

    expect(store.items()).toEqual([storedPizza]);
    expect(store.totalQuantity()).toBe(2);
  });

  it('persists changes so a new store restores the cart', () => {
    const store = createStore();
    store.addItem(PIZZA);
    store.increaseQuantity(PIZZA.menuItemId);
    TestBed.tick();

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
    const restoredStore = createStore();

    expect(restoredStore.items()).toEqual([{ ...PIZZA, quantity: 2 }]);
  });

  it('ignores invalid stored entries and strips extra fields', () => {
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify([
      { ...PIZZA, quantity: 0 },
      { ...SALAD, quantity: MAX_CART_ITEM_QUANTITY + 1 },
      { ...PIZZA, quantity: 2, description: 'Not part of the cart model' }
    ]));

    const store = createStore();

    expect(store.items()).toEqual([{ ...PIZZA, quantity: 2 }]);
    expect(Object.hasOwn(store.items()[0] ?? {}, 'description')).toBe(false);
  });

  it('handles malformed localStorage JSON without crashing', () => {
    localStorage.setItem(CART_STORAGE_KEY, '{invalid-json');

    expect(() => createStore()).not.toThrow();
    expect(createStore().items()).toEqual([]);
  });
});
