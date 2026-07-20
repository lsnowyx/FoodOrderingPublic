import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  Observable,
  Subject,
  catchError,
  combineLatest,
  distinctUntilChanged,
  map,
  of,
  startWith,
  switchMap
} from 'rxjs';

import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import {
  getOrderStatusLabel,
  getPaymentMethodLabel,
  getPaymentStatusLabel
} from '../../tracking/models/tracking-view-state';
import { LoadState } from '../../../shared/models/load-state';
import { PagedResponse } from '../../../shared/models/paged-response';
import { OrderSummary } from '../models/order-summary';
import { OrderService } from '../services/order.service';

function readPage(value: string | null): number {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [AsyncPipe, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './order-list.html',
  styleUrl: './order-list.scss'
})
export class OrderList {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly reload = new Subject<void>();

  protected readonly orderStatusLabel = getOrderStatusLabel;
  protected readonly paymentMethodLabel = getPaymentMethodLabel;
  protected readonly paymentStatusLabel = getPaymentStatusLabel;

  protected readonly ordersState$: Observable<LoadState<PagedResponse<OrderSummary>>> =
    combineLatest([
      this.route.queryParamMap.pipe(
        map(parameters => readPage(parameters.get('page'))),
        distinctUntilChanged()
      ),
      this.reload.pipe(startWith(undefined))
    ]).pipe(
      switchMap(([page]) => this.orderService.getMyOrders(page).pipe(
        map(data => ({ status: 'success', data }) as const),
        catchError(error => of({
          status: 'error',
          message: getApiErrorMessage(error, 'Your orders could not be loaded. Please try again.')
        } as const)),
        startWith({ status: 'loading' } as const)
      ))
    );

  protected retry(): void {
    this.reload.next();
  }

  protected shortOrderNumber(orderId: string): string {
    return orderId.slice(0, 8).toUpperCase();
  }
}
