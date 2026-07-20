import { Injectable, signal } from '@angular/core';

import { GuestOrderReference } from '../models/guest-order-reference';

export const GUEST_ORDER_STORAGE_KEY = 'food-ordering-guest-order-v1';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function getLocalStorage(): Storage | null {
  try {
    return typeof localStorage === 'undefined' ? null : localStorage;
  } catch {
    return null;
  }
}

function parseReference(value: unknown): GuestOrderReference | null {
  if (typeof value !== 'object' || value === null) {
    return null;
  }

  const reference = value as Record<string, unknown>;
  const orderId = reference['orderId'];
  const trackingToken = reference['trackingToken'];

  if (
    typeof orderId !== 'string' ||
    !GUID_PATTERN.test(orderId) ||
    typeof trackingToken !== 'string' ||
    trackingToken.trim().length === 0 ||
    trackingToken.length > 512
  ) {
    return null;
  }

  return { orderId, trackingToken };
}

function restoreReference(storage: Storage | null): GuestOrderReference | null {
  if (storage === null) {
    return null;
  }

  try {
    const storedValue = storage.getItem(GUEST_ORDER_STORAGE_KEY);
    return storedValue === null ? null : parseReference(JSON.parse(storedValue));
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class GuestOrderStorage {
  private readonly storage = getLocalStorage();
  private readonly writableReference = signal<GuestOrderReference | null>(
    restoreReference(this.storage)
  );

  readonly reference = this.writableReference.asReadonly();

  save(reference: GuestOrderReference): void {
    const validReference = parseReference(reference);
    if (validReference === null) {
      return;
    }

    this.writableReference.set(validReference);

    try {
      this.storage?.setItem(GUEST_ORDER_STORAGE_KEY, JSON.stringify(validReference));
    } catch {
      // The current session still retains the reference when storage is unavailable.
    }
  }

  clear(): void {
    this.writableReference.set(null);

    try {
      this.storage?.removeItem(GUEST_ORDER_STORAGE_KEY);
    } catch {
      // The in-memory reference is still cleared when storage is unavailable.
    }
  }
}
