import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  IHttpConnectionOptions,
  LogLevel
} from '@microsoft/signalr';

import { AuthStore } from '../../auth/services/auth-store.service';
import {
  CourierLocation,
  parseCourierLocation
} from '../models/courier-location';
import { TrackingConnectionState } from '../models/tracking-view-state';

const DELIVERY_TRACKING_HUB_URL = '/hubs/delivery-tracking';
const GUEST_SUBSCRIPTION_METHOD = 'SubscribeAsGuest';
const CUSTOMER_SUBSCRIPTION_METHOD = 'SubscribeAsCustomer';
const COURIER_LOCATION_EVENT = 'CourierLocationUpdated';

type TrackingConnectionMode = 'guest' | 'customer';

function getConnectionErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;
}

@Injectable({ providedIn: 'root' })
export class DeliverySignalRService {
  private readonly authStore = inject(AuthStore);
  private connection: HubConnection | null = null;
  private activeOrderId: string | null = null;
  private activeGuestToken: string | null = null;
  private activeMode: TrackingConnectionMode | null = null;
  private generation = 0;

  private readonly writableCourierLocation = signal<CourierLocation | null>(null);
  private readonly writableConnectionState = signal<TrackingConnectionState>('disconnected');
  private readonly writableConnectionError = signal('');

  readonly courierLocation = this.writableCourierLocation.asReadonly();
  readonly connectionState = this.writableConnectionState.asReadonly();
  readonly connectionError = this.writableConnectionError.asReadonly();

  async startGuestConnection(orderId: string, trackingToken: string): Promise<void> {
    if (this.isMatchingConnection('guest', orderId, trackingToken)) {
      return;
    }

    await this.startConnection('guest', orderId, trackingToken);
  }

  async startCustomerConnection(orderId: string): Promise<void> {
    if (this.authStore.getValidToken() === null) {
      this.writableConnectionState.set('error');
      this.writableConnectionError.set('Your session expired. Log in again to use live tracking.');
      return;
    }

    if (this.isMatchingConnection('customer', orderId, null)) {
      return;
    }

    await this.startConnection('customer', orderId, null);
  }

  async stopConnection(): Promise<void> {
    const connection = this.connection;
    this.generation += 1;
    this.connection = null;
    this.activeOrderId = null;
    this.activeGuestToken = null;
    this.activeMode = null;
    this.writableCourierLocation.set(null);
    this.writableConnectionState.set('disconnected');
    this.writableConnectionError.set('');

    if (connection === null) {
      return;
    }

    connection.off(COURIER_LOCATION_EVENT);
    try {
      await connection.stop();
    } catch {
      // The active page can continue with its polling fallback.
    }
  }

  private async startConnection(
    mode: TrackingConnectionMode,
    orderId: string,
    guestToken: string | null
  ): Promise<void> {
    await this.stopConnection();

    const generation = ++this.generation;
    const options: IHttpConnectionOptions | undefined = mode === 'customer'
      ? { accessTokenFactory: () => this.authStore.getValidToken() ?? '' }
      : undefined;
    const builder = new HubConnectionBuilder();
    const connection = options === undefined
      ? builder.withUrl(DELIVERY_TRACKING_HUB_URL)
      : builder.withUrl(DELIVERY_TRACKING_HUB_URL, options);
    const hubConnection = connection
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection = hubConnection;
    this.activeOrderId = orderId;
    this.activeGuestToken = guestToken;
    this.activeMode = mode;
    this.writableCourierLocation.set(null);
    this.writableConnectionError.set('');
    this.registerHandlers(hubConnection, generation);
    this.writableConnectionState.set('connecting');

    try {
      await hubConnection.start();
      if (!this.isCurrent(hubConnection, generation)) {
        await hubConnection.stop();
        return;
      }

      await this.subscribe(hubConnection, mode, orderId, guestToken);
      if (this.isCurrent(hubConnection, generation)) {
        this.writableConnectionState.set('connected');
        this.writableConnectionError.set('');
      }
    } catch (error: unknown) {
      await this.handleStartFailure(hubConnection, generation, error);
    }
  }

  private registerHandlers(connection: HubConnection, generation: number): void {
    connection.on(COURIER_LOCATION_EVENT, (payload: unknown) => {
      if (!this.isCurrent(connection, generation)) {
        return;
      }

      const location = parseCourierLocation(payload);
      if (location === null || location.orderId !== this.activeOrderId) {
        return;
      }
      this.writableCourierLocation.set(location);
    });

    connection.onreconnecting(error => {
      if (!this.isCurrent(connection, generation)) {
        return;
      }
      this.writableConnectionState.set('reconnecting');
      this.writableConnectionError.set(getConnectionErrorMessage(
        error,
        'Live delivery tracking is reconnecting.'
      ));
    });

    connection.onreconnected(() => {
      if (!this.isCurrent(connection, generation)) {
        return;
      }

      const orderId = this.activeOrderId;
      const mode = this.activeMode;
      if (orderId === null || mode === null) {
        return;
      }

      void this.subscribe(connection, mode, orderId, this.activeGuestToken).then(() => {
        if (this.isCurrent(connection, generation)) {
          this.writableConnectionState.set('connected');
          this.writableConnectionError.set('');
        }
      }).catch((error: unknown) => {
        if (this.isCurrent(connection, generation)) {
          this.writableConnectionState.set('error');
          this.writableConnectionError.set(getConnectionErrorMessage(
            error,
            'Live tracking reconnected but the order subscription failed.'
          ));
        }
      });
    });

    connection.onclose(error => {
      if (!this.isCurrent(connection, generation)) {
        return;
      }

      this.connection = null;
      this.activeOrderId = null;
      this.activeGuestToken = null;
      this.activeMode = null;
      this.writableConnectionState.set('disconnected');
      this.writableConnectionError.set(error === undefined
        ? ''
        : getConnectionErrorMessage(error, 'Live delivery tracking disconnected.'));
    });
  }

  private subscribe(
    connection: HubConnection,
    mode: TrackingConnectionMode,
    orderId: string,
    guestToken: string | null
  ): Promise<void> {
    if (mode === 'customer') {
      return connection.invoke<void>(CUSTOMER_SUBSCRIPTION_METHOD, orderId);
    }

    if (guestToken === null) {
      return Promise.reject(new Error('Guest tracking token is missing.'));
    }
    return connection.invoke<void>(GUEST_SUBSCRIPTION_METHOD, orderId, guestToken);
  }

  private async handleStartFailure(
    connection: HubConnection,
    generation: number,
    error: unknown
  ): Promise<void> {
    if (!this.isCurrent(connection, generation)) {
      return;
    }

    connection.off(COURIER_LOCATION_EVENT);
    this.connection = null;
    this.activeOrderId = null;
    this.activeGuestToken = null;
    this.activeMode = null;
    this.writableConnectionState.set('error');
    this.writableConnectionError.set(getConnectionErrorMessage(
      error,
      'Live delivery tracking could not be started.'
    ));

    try {
      await connection.stop();
    } catch {
      // Polling remains available when the connection cannot be stopped cleanly.
    }
  }

  private isMatchingConnection(
    mode: TrackingConnectionMode,
    orderId: string,
    guestToken: string | null
  ): boolean {
    return this.connection !== null
      && this.activeMode === mode
      && this.activeOrderId === orderId
      && this.activeGuestToken === guestToken
      && this.writableConnectionState() !== 'error'
      && this.writableConnectionState() !== 'disconnected';
  }

  private isCurrent(connection: HubConnection, generation: number): boolean {
    return this.connection === connection && this.generation === generation;
  }
}
