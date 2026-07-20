import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { GuestPaymentLinkResponse } from '../models/guest-payment-link-response';
import {
  GuestTrackingItem,
  GuestTrackingResponse
} from '../models/guest-tracking-response';
import {
  TrackingMapResponse,
  parseTrackingMapResponse
} from '../models/tracking-map-response';

const TRACKING_API_URL = '/api/tracking';
const GUEST_PAYMENT_LINK_API_URL = '/api/tracking/guest-orders/payment-link';
const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isDateOrNull(value: unknown): value is string | null {
  return value === null
    || (typeof value === 'string' && !Number.isNaN(Date.parse(value)));
}

function isNonNegativeNumber(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value) && value >= 0;
}

function parseTrackingItem(value: unknown): GuestTrackingItem {
  if (!isRecord(value)) {
    throw new Error('Malformed guest-tracking response.');
  }

  const menuItemId = value['menuItemId'];
  const menuItemName = value['menuItemName'];
  const quantity = value['quantity'];
  const unitPrice = value['unitPrice'];
  const lineTotal = value['lineTotal'];
  const totalCalories = value['totalCalories'];

  if (
    typeof menuItemId !== 'string'
    || !GUID_PATTERN.test(menuItemId)
    || typeof menuItemName !== 'string'
    || menuItemName.trim().length === 0
    || typeof quantity !== 'number'
    || !Number.isInteger(quantity)
    || quantity < 1
    || !isNonNegativeNumber(unitPrice)
    || !isNonNegativeNumber(lineTotal)
    || !isNonNegativeNumber(totalCalories)
  ) {
    throw new Error('Malformed guest-tracking response.');
  }

  return {
    menuItemId,
    menuItemName: menuItemName.trim(),
    quantity,
    unitPrice,
    lineTotal,
    totalCalories
  };
}

function parseGuestTrackingResponse(value: unknown): GuestTrackingResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed guest-tracking response.');
  }

  const orderId = value['orderId'];
  const status = value['status'];
  const deliveryStatus = value['deliveryStatus'];
  const paymentMethod = value['paymentMethod'];
  const paymentStatus = value['paymentStatus'];
  const isPaid = value['isPaid'];
  const canPayOnlineAgain = value['canPayOnlineAgain'];
  const paymentMessage = value['paymentMessage'];
  const updatedAt = value['updatedAt'];
  const deliveredAt = value['deliveredAt'];
  const items = value['items'];

  if (
    typeof orderId !== 'string'
    || !GUID_PATTERN.test(orderId)
    || typeof status !== 'string'
    || status.trim().length === 0
    || typeof deliveryStatus !== 'string'
    || deliveryStatus.trim().length === 0
    || typeof paymentMethod !== 'string'
    || paymentMethod.trim().length === 0
    || typeof paymentStatus !== 'string'
    || paymentStatus.trim().length === 0
    || typeof isPaid !== 'boolean'
    || typeof canPayOnlineAgain !== 'boolean'
    || typeof paymentMessage !== 'string'
    || paymentMessage.trim().length === 0
    || !isDateOrNull(updatedAt)
    || !isDateOrNull(deliveredAt)
    || !Array.isArray(items)
  ) {
    throw new Error('Malformed guest-tracking response.');
  }

  return {
    orderId,
    status,
    deliveryStatus,
    paymentMethod,
    paymentStatus,
    isPaid,
    canPayOnlineAgain,
    paymentMessage,
    updatedAt,
    deliveredAt,
    items: items.map(parseTrackingItem)
  };
}

function parseGuestPaymentLinkResponse(value: unknown): GuestPaymentLinkResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed guest payment-link response.');
  }

  const isPaid = value['isPaid'];
  const paymentUrl = value['paymentUrl'];
  const message = value['message'];

  if (
    typeof isPaid !== 'boolean'
    || !(paymentUrl === null || typeof paymentUrl === 'string')
    || typeof message !== 'string'
    || message.trim().length === 0
  ) {
    throw new Error('Malformed guest payment-link response.');
  }

  return { isPaid, paymentUrl, message };
}

@Injectable({ providedIn: 'root' })
export class TrackingService {
  private readonly http = inject(HttpClient);

  getGuestTracking(token: string): Observable<GuestTrackingResponse> {
    return this.http.get<unknown>(TRACKING_API_URL, {
      params: new HttpParams().set('token', token)
    }).pipe(map(parseGuestTrackingResponse));
  }

  getGuestMap(token: string): Observable<TrackingMapResponse> {
    return this.http.get<unknown>(`${TRACKING_API_URL}/map`, {
      params: new HttpParams().set('token', token)
    }).pipe(map(parseTrackingMapResponse));
  }

  getOrCreateGuestPaymentLink(token: string): Observable<GuestPaymentLinkResponse> {
    return this.http.post<unknown>(GUEST_PAYMENT_LINK_API_URL, null, {
      params: new HttpParams().set('token', token)
    }).pipe(map(parseGuestPaymentLinkResponse));
  }
}
