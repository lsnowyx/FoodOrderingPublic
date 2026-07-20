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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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

import { GuestOrderStorage } from '../../checkout/services/guest-order-storage.service';
import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import { DeliveryMap } from '../delivery-map/delivery-map';
import { GuestTrackingResponse } from '../models/guest-tracking-response';
import { TrackingMapResponse } from '../models/tracking-map-response';
import {
  getOrderStatusLabel,
  getPaymentMethodLabel,
  getPaymentStatusLabel,
  getTrackingStatusLabel,
  isTerminalOrderStatus
} from '../models/tracking-view-state';
import { OrderStatusTimeline } from '../order-status-timeline/order-status-timeline';
import { DeliverySignalRService } from '../services/delivery-signalr.service';
import { TrackingService } from '../services/tracking.service';

const TRACKING_POLL_INTERVAL_MS = 15_000;

interface ResolvedTrackingToken {
  readonly token: string | null;
  readonly shouldWriteStoredTokenToUrl: boolean;
}

type InitialTrackingResult =
  | {
    readonly kind: 'success';
    readonly token: string;
    readonly data: GuestTrackingResponse;
  }
  | {
    readonly kind: 'error';
    readonly token: string;
    readonly error: unknown;
  };

function normalizeToken(value: string | null): string | null {
  if (value === null) {
    return null;
  }

  const token = value.trim();
  return token.length > 0 ? token : null;
}

function isValidPaymentUrl(value: string | null): value is string {
  if (value === null || value.trim().length === 0) {
    return false;
  }

  try {
    const url = new URL(value);
    return url.protocol === 'https:' || url.protocol === 'http:';
  } catch {
    return false;
  }
}

@Component({
  selector: 'app-tracking-page',
  standalone: true,
  imports: [DatePipe, DecimalPipe, DeliveryMap, OrderStatusTimeline, RouterLink],
  templateUrl: './tracking-page.html',
  styleUrl: './tracking-page.scss'
})
export class TrackingPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly trackingService = inject(TrackingService);
  private readonly guestOrderStorage = inject(GuestOrderStorage);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly deliverySignalR = inject(DeliverySignalRService);
  protected readonly activeToken = signal<string | null>(null);
  protected readonly trackingData = signal<GuestTrackingResponse | null>(null);
  protected readonly mapData = signal<TrackingMapResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isMapLoading = signal(false);
  protected readonly isRefreshing = signal(false);
  protected readonly isPolling = signal(false);
  protected readonly trackingError = signal('');
  protected readonly mapError = signal('');
  protected readonly refreshError = signal('');
  protected readonly accessUnavailable = signal(false);
  protected readonly manualToken = signal('');
  protected readonly tokenEntryError = signal('');
  protected readonly isRetryingPayment = signal(false);
  protected readonly paymentRetryError = signal('');
  protected readonly paymentRetryMessage = signal('');

  protected readonly orderTotal = computed(() =>
    this.trackingData()?.items.reduce((total, item) => total + item.lineTotal, 0) ?? 0
  );
  protected readonly orderCalories = computed(() =>
    this.trackingData()?.items.reduce((total, item) => total + item.totalCalories, 0) ?? 0
  );
  protected readonly shortOrderNumber = computed(() =>
    this.trackingData()?.orderId.slice(0, 8).toUpperCase() ?? ''
  );
  protected readonly etaLabel = computed(() => {
    const liveLocation = this.deliverySignalR.courierLocation();
    const seconds = liveLocation?.estimatedSecondsRemaining
      ?? this.mapData()?.estimatedSecondsRemaining
      ?? null;

    if (seconds === null) {
      return null;
    }

    if (seconds === 0) {
      return 'Arrived';
    }

    return `${Math.max(1, Math.ceil(seconds / 60))} min`;
  });
  protected readonly deliveryProgressPercent = computed(() => {
    const progress = this.deliverySignalR.courierLocation()?.progress
      ?? this.mapData()?.progress
      ?? 0;
    return Math.round(progress * 100);
  });
  protected readonly connectionLabel = computed(() => {
    if (this.trackingData() !== null && isTerminalOrderStatus(this.trackingData()!.status)) {
      return 'Tracking complete';
    }

    switch (this.deliverySignalR.connectionState()) {
      case 'connected':
        return 'Live courier updates connected';
      case 'connecting':
        return 'Connecting to live courier updates';
      case 'reconnecting':
        return 'Reconnecting to live courier updates';
      case 'error':
        return 'Live updates unavailable';
      default:
        return this.isPolling() ? 'Status polling active' : 'Live updates not active';
    }
  });

  protected readonly orderStatusLabel = getOrderStatusLabel;
  protected readonly paymentMethodLabel = getPaymentMethodLabel;
  protected readonly paymentStatusLabel = getPaymentStatusLabel;
  protected readonly trackingStatusLabel = getTrackingStatusLabel;

  private pollingSubscription: Subscription | null = null;
  private pollingToken: string | null = null;
  private mapSubscription: Subscription | null = null;
  private mapRequestGeneration = 0;

  constructor() {
    this.route.queryParamMap.pipe(
      map(parameters => this.resolveToken(parameters.get('token'))),
      tap(resolved => {
        if (resolved.shouldWriteStoredTokenToUrl && resolved.token !== null) {
          void this.router.navigate(['/track'], {
            queryParams: { token: resolved.token },
            replaceUrl: true
          });
        }
      }),
      map(resolved => resolved.token),
      distinctUntilChanged(),
      switchMap(token => {
        this.prepareForToken(token);
        return token === null ? EMPTY : this.requestInitialTracking(token);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => this.handleInitialResult(result));

    effect(() => {
      const location = this.deliverySignalR.courierLocation();
      const trackingData = this.trackingData();
      if (location === null || trackingData === null || location.orderId !== trackingData.orderId) {
        return;
      }

      this.mapData.update(currentMap => currentMap?.orderId === location.orderId
        ? {
          ...currentMap,
          courierLatitude: location.latitude,
          courierLongitude: location.longitude,
          progress: location.progress,
          trackingStatus: location.trackingStatus,
          estimatedSecondsRemaining: location.estimatedSecondsRemaining,
          updatedAt: location.updatedAt,
          message: null
        }
        : currentMap);

      if (location.trackingStatus === 'Arrived' || location.trackingStatus === 'Cancelled') {
        void this.deliverySignalR.stopConnection();
      }
    });

    effect(() => {
      const token = this.activeToken();
      const data = this.trackingData();
      const currentMap = this.mapData();
      const connectionState = this.deliverySignalR.connectionState();
      const refreshing = this.isRefreshing();
      const unavailable = this.accessUnavailable();

      if (
        token === null
        || data === null
        || refreshing
        || unavailable
        || isTerminalOrderStatus(data.status)
      ) {
        this.stopPolling();
        return;
      }

      const deliverySessionFinished = currentMap?.trackingStatus === 'Arrived'
        || currentMap?.trackingStatus === 'Cancelled';
      const liveConnectionActive = data.status === 'OutForDelivery'
        && !deliverySessionFinished
        && connectionState === 'connected';

      if (liveConnectionActive) {
        this.stopPolling();
      } else {
        this.startPolling(token);
      }
    });

    this.destroyRef.onDestroy(() => {
      this.stopPolling();
      this.mapSubscription?.unsubscribe();
      void this.deliverySignalR.stopConnection();
    });
  }

  protected manualTokenChanged(event: Event): void {
    const input = event.target;
    if (input instanceof HTMLInputElement) {
      this.manualToken.set(input.value);
      this.tokenEntryError.set('');
    }
  }

  protected submitToken(event: Event): void {
    event.preventDefault();
    const token = this.manualToken().trim();
    if (token.length === 0) {
      this.tokenEntryError.set('Enter a tracking token.');
      return;
    }

    this.tokenEntryError.set('');
    void this.router.navigate(['/track'], {
      queryParams: { token }
    });
  }

  protected useAnotherToken(): void {
    this.guestOrderStorage.clear();
    this.manualToken.set('');
    this.tokenEntryError.set('');
    void this.router.navigate(['/track']);
  }

  protected retryInitialLoad(): void {
    const token = this.activeToken();
    if (token === null || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.trackingError.set('');
    this.requestInitialTracking(token).pipe(
      take(1),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => this.handleInitialResult(result));
  }

  protected refreshStatus(): void {
    const token = this.activeToken();
    if (token === null || this.isLoading() || this.isRefreshing()) {
      return;
    }

    this.stopPolling();
    this.isRefreshing.set(true);
    this.refreshError.set('');

    this.trackingService.getGuestTracking(token).pipe(
      take(1),
      finalize(() => this.isRefreshing.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        if (this.activeToken() !== token) {
          return;
        }

        this.applyTrackingData(data, token);
        this.loadMap(token, false);
      },
      error: error => this.handleRefreshError(error)
    });
  }

  protected retryPayment(): void {
    const token = this.activeToken();
    const data = this.trackingData();
    if (
      token === null
      || data === null
      || !data.canPayOnlineAgain
      || this.isRetryingPayment()
    ) {
      return;
    }

    this.paymentRetryError.set('');
    this.paymentRetryMessage.set('');
    const paymentWindow = window.open('', '_blank');
    if (paymentWindow === null || paymentWindow.closed) {
      this.paymentRetryError.set(
        'The payment tab was blocked. Allow pop-ups for this site, then try again.'
      );
      return;
    }

    paymentWindow.opener = null;
    this.isRetryingPayment.set(true);

    this.trackingService.getOrCreateGuestPaymentLink(token).pipe(
      take(1),
      finalize(() => this.isRetryingPayment.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: response => {
        if (response.isPaid) {
          this.closePaymentWindow(paymentWindow);
          this.paymentRetryMessage.set(response.message);
          this.refreshStatus();
          return;
        }

        if (!isValidPaymentUrl(response.paymentUrl)) {
          this.closePaymentWindow(paymentWindow);
          this.paymentRetryError.set(
            'The backend did not return a valid payment page. Your order is still available here.'
          );
          return;
        }

        try {
          paymentWindow.opener = null;
          paymentWindow.location.href = response.paymentUrl;
          this.paymentRetryMessage.set(
            'Payment opened in a new tab. This tracking page will remain available.'
          );
        } catch {
          this.closePaymentWindow(paymentWindow);
          this.paymentRetryError.set(
            'The payment page could not be opened. Your order is still available here.'
          );
        }
      },
      error: error => {
        this.closePaymentWindow(paymentWindow);
        this.paymentRetryError.set(getApiErrorMessage(
          error,
          'A payment link could not be created. Please try again.'
        ));
      }
    });
  }

  private resolveToken(urlValue: string | null): ResolvedTrackingToken {
    const urlToken = normalizeToken(urlValue);
    if (urlToken !== null) {
      return { token: urlToken, shouldWriteStoredTokenToUrl: false };
    }

    const storedToken = this.guestOrderStorage.reference()?.trackingToken ?? null;
    return {
      token: storedToken,
      shouldWriteStoredTokenToUrl: storedToken !== null
    };
  }

  private prepareForToken(token: string | null): void {
    this.stopPolling();
    this.mapSubscription?.unsubscribe();
    this.mapSubscription = null;
    this.mapRequestGeneration += 1;
    void this.deliverySignalR.stopConnection();

    this.activeToken.set(token);
    this.trackingData.set(null);
    this.mapData.set(null);
    this.isLoading.set(token !== null);
    this.isMapLoading.set(false);
    this.isRefreshing.set(false);
    this.trackingError.set('');
    this.mapError.set('');
    this.refreshError.set('');
    this.accessUnavailable.set(false);
    this.paymentRetryError.set('');
    this.paymentRetryMessage.set('');
  }

  private requestInitialTracking(token: string): Observable<InitialTrackingResult> {
    return this.trackingService.getGuestTracking(token).pipe(
      map(data => ({ kind: 'success', token, data }) as const),
      catchError((error: unknown) => of({ kind: 'error', token, error } as const))
    );
  }

  private handleInitialResult(result: InitialTrackingResult): void {
    if (this.activeToken() !== result.token) {
      return;
    }

    this.isLoading.set(false);
    if (result.kind === 'error') {
      this.trackingError.set(this.getInitialTrackingError(result.error));
      return;
    }

    this.applyTrackingData(result.data, result.token);
    this.loadMap(result.token, true);
  }

  private applyTrackingData(data: GuestTrackingResponse, token: string): void {
    if (this.activeToken() !== token) {
      return;
    }

    this.trackingData.set(data);
    this.trackingError.set('');
    this.refreshError.set('');
    this.accessUnavailable.set(false);
    this.guestOrderStorage.save({
      orderId: data.orderId,
      trackingToken: token
    });
    this.configureLiveTracking(data, token);
  }

  private configureLiveTracking(data: GuestTrackingResponse, token: string): void {
    const deliverySessionFinished = this.mapData()?.trackingStatus === 'Arrived'
      || this.mapData()?.trackingStatus === 'Cancelled';

    if (
      isTerminalOrderStatus(data.status)
      || data.status !== 'OutForDelivery'
      || deliverySessionFinished
    ) {
      void this.deliverySignalR.stopConnection();
      return;
    }

    void this.deliverySignalR.startGuestConnection(data.orderId, token);
  }

  private loadMap(token: string, showLoading: boolean): void {
    const generation = ++this.mapRequestGeneration;
    this.mapSubscription?.unsubscribe();
    this.mapError.set('');
    if (showLoading) {
      this.isMapLoading.set(true);
    }

    this.mapSubscription = this.trackingService.getGuestMap(token).pipe(
      take(1),
      finalize(() => {
        if (this.mapRequestGeneration === generation) {
          this.isMapLoading.set(false);
          this.mapSubscription = null;
        }
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: data => {
        const trackingData = this.trackingData();
        if (
          this.activeToken() !== token
          || trackingData === null
          || data.orderId !== trackingData.orderId
        ) {
          return;
        }

        this.mapData.set(data);
        this.mapError.set('');
        if (data.trackingStatus === 'Arrived' || data.trackingStatus === 'Cancelled') {
          void this.deliverySignalR.stopConnection();
        } else {
          this.configureLiveTracking(trackingData, token);
        }
      },
      error: error => {
        if (this.activeToken() !== token) {
          return;
        }

        this.mapError.set(getApiErrorMessage(
          error,
          'Delivery map information is temporarily unavailable.'
        ));
      }
    });
  }

  private startPolling(token: string): void {
    if (this.pollingSubscription !== null && this.pollingToken === token) {
      return;
    }

    this.stopPolling();
    this.pollingToken = token;
    this.isPolling.set(true);
    this.pollingSubscription = timer(
      TRACKING_POLL_INTERVAL_MS,
      TRACKING_POLL_INTERVAL_MS
    ).pipe(
      exhaustMap(() => {
        if (this.activeToken() !== token || this.accessUnavailable()) {
          return EMPTY;
        }

        return this.trackingService.getGuestTracking(token).pipe(
          tap(data => {
            this.applyTrackingData(data, token);
            this.loadMap(token, false);
          }),
          catchError(error => {
            this.handleRefreshError(error);
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
    this.pollingToken = null;
    this.isPolling.set(false);
  }

  private handleRefreshError(error: unknown): void {
    if (this.isPermanentAccessError(error)) {
      this.accessUnavailable.set(true);
      this.refreshError.set(this.getEndedAccessMessage(error));
      this.stopPolling();
      void this.deliverySignalR.stopConnection();
      return;
    }

    this.refreshError.set(getApiErrorMessage(
      error,
      'The latest status could not be loaded. The last available information is still shown.'
    ));
  }

  private getInitialTrackingError(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'No order was found for this tracking token.';
    }

    if (error instanceof HttpErrorResponse && error.status === 403) {
      const detail = getApiErrorMessage(error, '').toLowerCase();
      if (detail.includes('expired')) {
        return 'This tracking token has expired.';
      }

      return 'This tracking token is no longer authorized.';
    }

    return getApiErrorMessage(
      error,
      'Order tracking could not be loaded. Check your connection and try again.'
    );
  }

  private getEndedAccessMessage(error: unknown): string {
    const detail = getApiErrorMessage(error, '').toLowerCase();
    if (detail.includes('expired')) {
      return 'This tracking token expired. The last available order information is preserved below.';
    }

    if (this.mapData()?.trackingStatus === 'Arrived') {
      return 'The courier reached the destination and guest tracking access has ended. The last available order information is preserved below.';
    }

    return 'Guest tracking access has ended or was revoked. The last available order information is preserved below.';
  }

  private isPermanentAccessError(error: unknown): boolean {
    return error instanceof HttpErrorResponse
      && (error.status === 403 || error.status === 404);
  }

  private closePaymentWindow(paymentWindow: Window): void {
    if (!paymentWindow.closed) {
      paymentWindow.close();
    }
  }
}
