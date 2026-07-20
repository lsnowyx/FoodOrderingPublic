import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  EMPTY,
  Observable,
  Subscription,
  catchError,
  distinctUntilChanged,
  exhaustMap,
  finalize,
  map,
  of,
  switchMap,
  take,
  tap,
  timer
} from 'rxjs';

import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import { DeliveryMap } from '../../tracking/delivery-map/delivery-map';
import { TrackingMapResponse } from '../../tracking/models/tracking-map-response';
import {
  getOrderStatusLabel,
  getPaymentMethodLabel,
  getPaymentStatusLabel,
  getTrackingStatusLabel,
  isTerminalOrderStatus
} from '../../tracking/models/tracking-view-state';
import { OrderStatusTimeline } from '../../tracking/order-status-timeline/order-status-timeline';
import { DeliverySignalRService } from '../../tracking/services/delivery-signalr.service';
import { OrderDetailsResponse } from '../models/order-details';
import { OrderService } from '../services/order.service';

const TRACKING_POLL_INTERVAL_MS = 15_000;
const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

type InitialOrderResult =
  | { readonly kind: 'success'; readonly orderId: string; readonly data: OrderDetailsResponse }
  | { readonly kind: 'error'; readonly orderId: string | null; readonly error: unknown };

@Component({
  selector: 'app-customer-tracking',
  standalone: true,
  imports: [DatePipe, DecimalPipe, DeliveryMap, OrderStatusTimeline, RouterLink],
  templateUrl: './customer-tracking.html',
  styleUrl: './customer-tracking.scss'
})
export class CustomerTracking {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly deliverySignalR = inject(DeliverySignalRService);
  protected readonly activeOrderId = signal<string | null>(null);
  protected readonly orderData = signal<OrderDetailsResponse | null>(null);
  protected readonly mapData = signal<TrackingMapResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isMapLoading = signal(false);
  protected readonly isRefreshing = signal(false);
  protected readonly isPolling = signal(false);
  protected readonly orderError = signal('');
  protected readonly mapError = signal('');
  protected readonly refreshError = signal('');

  protected readonly shortOrderNumber = computed(() =>
    this.orderData()?.orderId.slice(0, 8).toUpperCase() ?? ''
  );
  protected readonly etaLabel = computed(() => {
    const seconds = this.deliverySignalR.courierLocation()?.estimatedSecondsRemaining
      ?? this.mapData()?.estimatedSecondsRemaining
      ?? null;
    if (seconds === null) {
      return null;
    }
    return seconds === 0 ? 'Arrived' : `${Math.max(1, Math.ceil(seconds / 60))} min`;
  });
  protected readonly deliveryProgressPercent = computed(() => Math.round(
    (this.deliverySignalR.courierLocation()?.progress ?? this.mapData()?.progress ?? 0) * 100
  ));
  protected readonly connectionLabel = computed(() => {
    const order = this.orderData();
    if (order !== null && isTerminalOrderStatus(order.orderStatus)) {
      return 'Tracking complete';
    }

    switch (this.deliverySignalR.connectionState()) {
      case 'connected': return 'Live courier updates connected';
      case 'connecting': return 'Connecting to live courier updates';
      case 'reconnecting': return 'Reconnecting to live courier updates';
      case 'error': return 'Live updates unavailable';
      default: return this.isPolling() ? 'Status polling active' : 'Live updates not active';
    }
  });

  protected readonly orderStatusLabel = getOrderStatusLabel;
  protected readonly paymentMethodLabel = getPaymentMethodLabel;
  protected readonly paymentStatusLabel = getPaymentStatusLabel;
  protected readonly trackingStatusLabel = getTrackingStatusLabel;

  private pollingSubscription: Subscription | null = null;
  private pollingOrderId: string | null = null;
  private mapSubscription: Subscription | null = null;
  private mapRequestGeneration = 0;

  constructor() {
    this.route.paramMap.pipe(
      map(parameters => parameters.get('orderId')),
      distinctUntilChanged(),
      tap(orderId => this.prepareOrder(orderId)),
      switchMap(orderId => {
        if (orderId === null || !GUID_PATTERN.test(orderId)) {
          return of({
            kind: 'error',
            orderId,
            error: new Error('Invalid order ID.')
          } as const);
        }
        return this.requestInitialOrder(orderId);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => this.handleInitialResult(result));

    effect(() => {
      const orderId = this.activeOrderId();
      const order = this.orderData();
      const mapData = this.mapData();
      const liveLocation = this.deliverySignalR.courierLocation();
      const connectionState = this.deliverySignalR.connectionState();
      if (orderId === null || order === null || this.isRefreshing()) {
        this.stopPolling();
        return;
      }

      const mapFinished = mapData?.trackingStatus === 'Arrived'
        || mapData?.trackingStatus === 'Cancelled';
      const liveDeliveryFinished = liveLocation?.trackingStatus === 'Arrived'
        || liveLocation?.trackingStatus === 'Cancelled';
      if (isTerminalOrderStatus(order.orderStatus) || mapFinished) {
        this.stopPolling();
        if (connectionState !== 'disconnected') {
          void this.deliverySignalR.stopConnection();
        }
        return;
      }

      if (liveDeliveryFinished) {
        if (connectionState !== 'disconnected') {
          void this.deliverySignalR.stopConnection();
        }
        this.startPolling(orderId);
        return;
      }

      const liveConnected = order.orderStatus === 'OutForDelivery'
        && connectionState === 'connected';
      if (liveConnected) {
        this.stopPolling();
      } else {
        this.startPolling(orderId);
      }
    });

    this.destroyRef.onDestroy(() => {
      this.stopPolling();
      this.mapSubscription?.unsubscribe();
      void this.deliverySignalR.stopConnection();
    });
  }

  protected refreshStatus(): void {
    const orderId = this.activeOrderId();
    if (orderId === null || this.isLoading() || this.isRefreshing()) {
      return;
    }

    this.stopPolling();
    this.isRefreshing.set(true);
    this.refreshError.set('');
    this.orderService.getMyOrder(orderId).pipe(
      take(1),
      finalize(() => this.isRefreshing.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: order => {
        if (this.activeOrderId() !== orderId) {
          return;
        }
        this.applyOrder(order);
        this.loadMap(orderId, false);
      },
      error: error => this.refreshError.set(getApiErrorMessage(
        error,
        'The latest status could not be loaded. Existing information is preserved.'
      ))
    });
  }

  protected retryInitialLoad(): void {
    const orderId = this.activeOrderId();
    if (orderId === null || !GUID_PATTERN.test(orderId) || this.isLoading()) {
      return;
    }
    this.isLoading.set(true);
    this.orderError.set('');
    this.requestInitialOrder(orderId).pipe(
      take(1),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => this.handleInitialResult(result));
  }

  private prepareOrder(orderId: string | null): void {
    this.stopPolling();
    this.mapSubscription?.unsubscribe();
    this.mapSubscription = null;
    this.mapRequestGeneration += 1;
    void this.deliverySignalR.stopConnection();
    this.activeOrderId.set(orderId);
    this.orderData.set(null);
    this.mapData.set(null);
    this.isLoading.set(orderId !== null);
    this.isMapLoading.set(false);
    this.orderError.set('');
    this.mapError.set('');
    this.refreshError.set('');
  }

  private requestInitialOrder(orderId: string): Observable<InitialOrderResult> {
    return this.orderService.getMyOrder(orderId).pipe(
      map(data => ({ kind: 'success', orderId, data }) as const),
      catchError((error: unknown) => of({ kind: 'error', orderId, error } as const))
    );
  }

  private handleInitialResult(result: InitialOrderResult): void {
    if (this.activeOrderId() !== result.orderId) {
      return;
    }

    this.isLoading.set(false);
    if (result.kind === 'error') {
      this.orderError.set(this.getOrderError(result.error));
      return;
    }

    this.applyOrder(result.data);
    this.loadMap(result.orderId, true);
  }

  private applyOrder(order: OrderDetailsResponse): void {
    if (this.activeOrderId() !== order.orderId) {
      return;
    }
    this.orderData.set(order);
    this.orderError.set('');
    this.refreshError.set('');
    this.configureLiveTracking(order);
  }

  private configureLiveTracking(order: OrderDetailsResponse): void {
    const mapFinished = this.mapData()?.trackingStatus === 'Arrived'
      || this.mapData()?.trackingStatus === 'Cancelled';
    if (
      isTerminalOrderStatus(order.orderStatus)
      || order.orderStatus !== 'OutForDelivery'
      || mapFinished
    ) {
      void this.deliverySignalR.stopConnection();
      return;
    }
    void this.deliverySignalR.startCustomerConnection(order.orderId);
  }

  private loadMap(orderId: string, showLoading: boolean): void {
    const generation = ++this.mapRequestGeneration;
    this.mapSubscription?.unsubscribe();
    this.mapError.set('');
    if (showLoading) {
      this.isMapLoading.set(true);
    }

    this.mapSubscription = this.orderService.getMyOrderMap(orderId).pipe(
      take(1),
      finalize(() => {
        if (this.mapRequestGeneration === generation) {
          this.isMapLoading.set(false);
          this.mapSubscription = null;
        }
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: mapData => {
        if (this.activeOrderId() !== orderId || mapData.orderId !== orderId) {
          return;
        }
        this.mapData.set(mapData);
        this.mapError.set('');
        const order = this.orderData();
        if (order !== null) {
          this.configureLiveTracking(order);
        }
      },
      error: error => {
        if (this.activeOrderId() === orderId) {
          this.mapError.set(getApiErrorMessage(
            error,
            'Delivery map information is temporarily unavailable.'
          ));
        }
      }
    });
  }

  private startPolling(orderId: string): void {
    if (this.pollingSubscription !== null && this.pollingOrderId === orderId) {
      return;
    }

    this.stopPolling();
    this.pollingOrderId = orderId;
    this.isPolling.set(true);
    this.pollingSubscription = timer(
      TRACKING_POLL_INTERVAL_MS,
      TRACKING_POLL_INTERVAL_MS
    ).pipe(
      exhaustMap(() => {
        if (this.activeOrderId() !== orderId) {
          return EMPTY;
        }
        return this.orderService.getMyOrder(orderId).pipe(
          tap(order => {
            this.applyOrder(order);
            this.loadMap(orderId, false);
          }),
          catchError(error => {
            this.refreshError.set(getApiErrorMessage(
              error,
              'A polling refresh failed. The next refresh will still be attempted.'
            ));
            return EMPTY;
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  private stopPolling(): void {
    this.pollingSubscription?.unsubscribe();
    this.pollingSubscription = null;
    this.pollingOrderId = null;
    this.isPolling.set(false);
  }

  private getOrderError(error: unknown): string {
    if (error instanceof Error && error.message === 'Invalid order ID.') {
      return 'The order ID in the address is invalid.';
    }
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'This order was not found or does not belong to your account.';
    }
    if (error instanceof HttpErrorResponse && error.status === 403) {
      return 'Your account is not allowed to access this order.';
    }
    return getApiErrorMessage(error, 'Order tracking could not be loaded.');
  }
}
