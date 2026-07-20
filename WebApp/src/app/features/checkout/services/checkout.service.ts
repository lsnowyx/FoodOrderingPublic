import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { GuestCheckoutRequest } from '../models/guest-checkout-request';
import { GuestCheckoutResponse } from '../models/guest-checkout-response';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function parseGuestCheckoutResponse(value: unknown): GuestCheckoutResponse {
  if (typeof value !== 'object' || value === null) {
    throw new Error('Malformed guest-checkout response.');
  }

  const response = value as Record<string, unknown>;
  const orderId = response['orderId'];
  const status = response['status'];
  const createdAt = response['createdAt'];
  const total = response['total'];
  const trackingToken = response['trackingToken'];
  const paymentUrl = response['paymentUrl'];

  if (
    typeof orderId !== 'string' ||
    !GUID_PATTERN.test(orderId) ||
    typeof status !== 'string' ||
    status.trim().length === 0 ||
    typeof createdAt !== 'string' ||
    Number.isNaN(Date.parse(createdAt)) ||
    typeof total !== 'number' ||
    !Number.isFinite(total) ||
    total < 0 ||
    typeof trackingToken !== 'string' ||
    trackingToken.trim().length === 0 ||
    !(paymentUrl === null || typeof paymentUrl === 'string')
  ) {
    throw new Error('Malformed guest-checkout response.');
  }

  return {
    orderId,
    status,
    createdAt,
    total,
    trackingToken,
    paymentUrl
  };
}

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly http = inject(HttpClient);

  checkoutGuest(request: GuestCheckoutRequest): Observable<GuestCheckoutResponse> {
    return this.http.post<unknown>('/api/checkout/guest', request).pipe(
      map(parseGuestCheckoutResponse)
    );
  }
}
