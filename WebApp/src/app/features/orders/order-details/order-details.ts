import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  Observable,
  Subject,
  catchError,
  combineLatest,
  finalize,
  map,
  of,
  startWith,
  switchMap,
  take
} from 'rxjs';

import { LoadState } from '../../../shared/models/load-state';
import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import {
  getOrderStatusLabel,
  getPaymentMethodLabel,
  getPaymentStatusLabel
} from '../../tracking/models/tracking-view-state';
import { OrderDetailsResponse } from '../models/order-details';
import { OrderService } from '../services/order.service';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

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
  selector: 'app-order-details',
  standalone: true,
  imports: [AsyncPipe, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './order-details.html',
  styleUrl: './order-details.scss'
})
export class OrderDetails {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly reload = new Subject<void>();

  protected readonly isRetryingPayment = signal(false);
  protected readonly paymentError = signal('');
  protected readonly paymentMessage = signal('');
  protected readonly paymentOpened = this.route.snapshot.queryParamMap.get('paymentOpened') === 'true';
  protected readonly paymentIssue = this.route.snapshot.queryParamMap.get('paymentIssue');
  protected readonly orderStatusLabel = getOrderStatusLabel;
  protected readonly paymentMethodLabel = getPaymentMethodLabel;
  protected readonly paymentStatusLabel = getPaymentStatusLabel;

  protected readonly orderState$: Observable<LoadState<OrderDetailsResponse>> = combineLatest([
    this.route.paramMap.pipe(map(parameters => parameters.get('orderId'))),
    this.reload.pipe(startWith(undefined))
  ]).pipe(
    switchMap(([orderId]) => {
      if (orderId === null || !GUID_PATTERN.test(orderId)) {
        return of({ status: 'error', message: 'The order ID in the address is invalid.' } as const);
      }

      return this.orderService.getMyOrder(orderId).pipe(
        map(data => ({ status: 'success', data }) as const),
        catchError(error => of({
          status: 'error',
          message: this.getOrderLoadError(error)
        } as const)),
        startWith({ status: 'loading' } as const)
      );
    })
  );

  protected shortOrderNumber(orderId: string): string {
    return orderId.slice(0, 8).toUpperCase();
  }

  protected retryLoad(): void {
    this.reload.next();
  }

  protected retryPayment(order: OrderDetailsResponse): void {
    if (!order.canPayOnlineAgain || this.isRetryingPayment()) {
      return;
    }

    this.paymentError.set('');
    this.paymentMessage.set('');
    const paymentWindow = window.open('', '_blank');
    if (paymentWindow === null || paymentWindow.closed) {
      this.paymentError.set('Allow pop-ups for this site, then try again.');
      return;
    }

    paymentWindow.opener = null;
    this.isRetryingPayment.set(true);
    this.orderService.getPaymentLink(order.orderId).pipe(
      take(1),
      finalize(() => this.isRetryingPayment.set(false))
    ).subscribe({
      next: response => {
        if (response.isPaid) {
          this.closePaymentWindow(paymentWindow);
          this.paymentMessage.set(response.message);
          this.reload.next();
          return;
        }

        if (!response.canPayOnlineAgain || !isValidPaymentUrl(response.paymentUrl)) {
          this.closePaymentWindow(paymentWindow);
          this.paymentError.set(response.message || 'A payment page is not available for this order.');
          this.reload.next();
          return;
        }

        try {
          paymentWindow.opener = null;
          paymentWindow.location.href = response.paymentUrl;
          this.paymentMessage.set('Payment opened in a new tab. This order page remains open.');
        } catch {
          this.closePaymentWindow(paymentWindow);
          this.paymentError.set('The payment page could not be opened.');
        }
      },
      error: error => {
        this.closePaymentWindow(paymentWindow);
        this.paymentError.set(getApiErrorMessage(
          error,
          'A payment link could not be created. Please try again.'
        ));
      }
    });
  }

  private closePaymentWindow(paymentWindow: Window): void {
    if (!paymentWindow.closed) {
      paymentWindow.close();
    }
  }

  private getOrderLoadError(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'This order was not found or does not belong to your account.';
    }
    if (error instanceof HttpErrorResponse && error.status === 403) {
      return 'Your account is not allowed to view this order.';
    }
    return getApiErrorMessage(error, 'Order details could not be loaded.');
  }
}
