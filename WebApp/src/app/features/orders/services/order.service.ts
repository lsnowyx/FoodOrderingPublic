import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { PagedResponse } from '../../../shared/models/paged-response';
import {
  TrackingMapResponse,
  parseTrackingMapResponse
} from '../../tracking/models/tracking-map-response';
import { CreateOrderRequest } from '../models/create-order-request';
import { CreateOrderResponse } from '../models/create-order-response';
import { OrderDetailsItem, OrderDetailsResponse } from '../models/order-details';
import { OrderSummary } from '../models/order-summary';
import { PaymentLinkResponse } from '../models/payment-link-response';

const ORDERS_API_URL = '/api/orders';
const FULL_GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isDate(value: unknown): value is string {
  return typeof value === 'string' && !Number.isNaN(Date.parse(value));
}

function isDateOrNull(value: unknown): value is string | null {
  return value === null || isDate(value);
}

function isNonNegativeNumber(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value) && value >= 0;
}

function isString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

function parseCreateOrderResponse(value: unknown): CreateOrderResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed order-creation response.');
  }

  const orderId = value['orderId'];
  const status = value['status'];
  const isPaid = value['isPaid'];
  const paymentMethod = value['paymentMethod'];
  const paymentStatus = value['paymentStatus'];
  const totalAmount = value['totalAmount'];
  const paymentUrl = value['paymentUrl'];
  if (
    typeof orderId !== 'string'
    || !FULL_GUID_PATTERN.test(orderId)
    || !isString(status)
    || typeof isPaid !== 'boolean'
    || !isString(paymentMethod)
    || !isString(paymentStatus)
    || !isNonNegativeNumber(totalAmount)
    || !(paymentUrl === null || typeof paymentUrl === 'string')
  ) {
    throw new Error('Malformed order-creation response.');
  }

  return {
    orderId,
    status,
    isPaid,
    paymentMethod,
    paymentStatus,
    totalAmount,
    paymentUrl
  };
}

function parseOrderSummary(value: unknown): OrderSummary {
  if (!isRecord(value)) {
    throw new Error('Malformed order-list response.');
  }

  const orderId = value['orderId'];
  const createdAt = value['createdAt'];
  const orderStatus = value['orderStatus'];
  const paymentMethod = value['paymentMethod'];
  const paymentStatus = value['paymentStatus'];
  const isPaid = value['isPaid'];
  const totalAmount = value['totalAmount'];
  const itemsCount = value['itemsCount'];
  const canPayOnlineAgain = value['canPayOnlineAgain'];
  const paymentMessage = value['paymentMessage'];
  const deliveredAt = value['deliveredAt'];

  if (
    typeof orderId !== 'string'
    || !FULL_GUID_PATTERN.test(orderId)
    || !isDate(createdAt)
    || !isString(orderStatus)
    || !isString(paymentMethod)
    || !isString(paymentStatus)
    || typeof isPaid !== 'boolean'
    || !isNonNegativeNumber(totalAmount)
    || typeof itemsCount !== 'number'
    || !Number.isInteger(itemsCount)
    || itemsCount < 0
    || typeof canPayOnlineAgain !== 'boolean'
    || !isString(paymentMessage)
    || !isDateOrNull(deliveredAt)
  ) {
    throw new Error('Malformed order-list response.');
  }

  return {
    orderId,
    createdAt,
    orderStatus,
    paymentMethod,
    paymentStatus,
    isPaid,
    totalAmount,
    itemsCount,
    canPayOnlineAgain,
    paymentMessage,
    deliveredAt
  };
}

function parsePagedOrders(value: unknown): PagedResponse<OrderSummary> {
  if (!isRecord(value) || !Array.isArray(value['items'])) {
    throw new Error('Malformed order-list response.');
  }

  const page = value['page'];
  const pageSize = value['pageSize'];
  const totalCount = value['totalCount'];
  const totalPages = value['totalPages'];
  if (
    typeof page !== 'number'
    || !Number.isInteger(page)
    || page < 1
    || typeof pageSize !== 'number'
    || !Number.isInteger(pageSize)
    || pageSize < 1
    || typeof totalCount !== 'number'
    || !Number.isInteger(totalCount)
    || totalCount < 0
    || typeof totalPages !== 'number'
    || !Number.isInteger(totalPages)
    || totalPages < 0
  ) {
    throw new Error('Malformed order-list response.');
  }

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    items: value['items'].map(parseOrderSummary)
  };
}

function parseOrderItem(value: unknown): OrderDetailsItem {
  if (!isRecord(value)) {
    throw new Error('Malformed order-details response.');
  }

  const menuItemId = value['menuItemId'];
  const menuItemName = value['menuItemName'];
  const quantity = value['quantity'];
  const unitPrice = value['unitPrice'];
  const lineTotal = value['lineTotal'];
  const totalCalories = value['totalCalories'];
  if (
    typeof menuItemId !== 'string'
    || !FULL_GUID_PATTERN.test(menuItemId)
    || !isString(menuItemName)
    || typeof quantity !== 'number'
    || !Number.isInteger(quantity)
    || quantity < 1
    || !isNonNegativeNumber(unitPrice)
    || !isNonNegativeNumber(lineTotal)
    || !isNonNegativeNumber(totalCalories)
  ) {
    throw new Error('Malformed order-details response.');
  }

  return { menuItemId, menuItemName, quantity, unitPrice, lineTotal, totalCalories };
}

function parseCoordinate(value: unknown, minimum: number, maximum: number): value is number | null {
  return value === null
    || (typeof value === 'number'
      && Number.isFinite(value)
      && value >= minimum
      && value <= maximum);
}

function parseOrderDetails(value: unknown): OrderDetailsResponse {
  if (!isRecord(value) || !Array.isArray(value['items'])) {
    throw new Error('Malformed order-details response.');
  }

  const orderId = value['orderId'];
  const createdAt = value['createdAt'];
  const orderStatus = value['orderStatus'];
  const paymentMethod = value['paymentMethod'];
  const paymentStatus = value['paymentStatus'];
  const isPaid = value['isPaid'];
  const totalAmount = value['totalAmount'];
  const paidAt = value['paidAt'];
  const deliveredAt = value['deliveredAt'];
  const updatedAt = value['updatedAt'];
  const deliveryAddress = value['deliveryAddress'];
  const deliveryLatitude = value['deliveryLatitude'];
  const deliveryLongitude = value['deliveryLongitude'];
  const canPayOnlineAgain = value['canPayOnlineAgain'];
  const paymentMessage = value['paymentMessage'];

  if (
    typeof orderId !== 'string'
    || !FULL_GUID_PATTERN.test(orderId)
    || !isDate(createdAt)
    || !isString(orderStatus)
    || !isString(paymentMethod)
    || !isString(paymentStatus)
    || typeof isPaid !== 'boolean'
    || !isNonNegativeNumber(totalAmount)
    || !isDateOrNull(paidAt)
    || !isDateOrNull(deliveredAt)
    || !isDateOrNull(updatedAt)
    || !(deliveryAddress === null || typeof deliveryAddress === 'string')
    || !parseCoordinate(deliveryLatitude, -90, 90)
    || !parseCoordinate(deliveryLongitude, -180, 180)
    || typeof canPayOnlineAgain !== 'boolean'
    || !isString(paymentMessage)
  ) {
    throw new Error('Malformed order-details response.');
  }

  return {
    orderId,
    createdAt,
    orderStatus,
    paymentMethod,
    paymentStatus,
    isPaid,
    totalAmount,
    paidAt,
    deliveredAt,
    updatedAt,
    deliveryAddress,
    deliveryLatitude,
    deliveryLongitude,
    canPayOnlineAgain,
    paymentMessage,
    items: value['items'].map(parseOrderItem)
  };
}

function parsePaymentLinkResponse(value: unknown): PaymentLinkResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed payment-link response.');
  }

  const isPaid = value['isPaid'];
  const canPayOnlineAgain = value['canPayOnlineAgain'];
  const paymentUrl = value['paymentUrl'];
  const message = value['message'];
  const paymentStatus = value['paymentStatus'];
  if (
    typeof isPaid !== 'boolean'
    || typeof canPayOnlineAgain !== 'boolean'
    || !(paymentUrl === null || typeof paymentUrl === 'string')
    || !isString(message)
    || !isString(paymentStatus)
  ) {
    throw new Error('Malformed payment-link response.');
  }

  return { isPaid, canPayOnlineAgain, paymentUrl, message, paymentStatus };
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<unknown>(ORDERS_API_URL, request).pipe(
      map(parseCreateOrderResponse)
    );
  }

  getMyOrders(page: number, pageSize = 12): Observable<PagedResponse<OrderSummary>> {
    return this.http.get<unknown>(`${ORDERS_API_URL}/my`, {
      params: new HttpParams()
        .set('page', page)
        .set('pageSize', pageSize)
    }).pipe(map(parsePagedOrders));
  }

  getMyOrder(orderId: string): Observable<OrderDetailsResponse> {
    return this.http.get<unknown>(`${ORDERS_API_URL}/my/${orderId}`).pipe(
      map(parseOrderDetails)
    );
  }

  getMyOrderMap(orderId: string): Observable<TrackingMapResponse> {
    return this.http.get<unknown>(`${ORDERS_API_URL}/my/${orderId}/map`).pipe(
      map(parseTrackingMapResponse)
    );
  }

  getPaymentLink(orderId: string): Observable<PaymentLinkResponse> {
    return this.http.post<unknown>(`${ORDERS_API_URL}/my/${orderId}/payment-link`, null).pipe(
      map(parsePaymentLinkResponse)
    );
  }
}
