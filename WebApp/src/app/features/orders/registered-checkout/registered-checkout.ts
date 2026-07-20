import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';

import { CartStore } from '../../cart/services/cart-store.service';
import { GuestPaymentMethod } from '../../checkout/models/guest-payment-method';
import { LocationSuggestion } from '../../checkout/models/location-suggestion';
import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import { LocationService } from '../../checkout/services/location.service';
import {
  CreateOrderRequest,
  MAX_ORDER_LINE_COUNT,
  MAX_ORDER_TOTAL_QUANTITY
} from '../models/create-order-request';
import { CreateOrderResponse } from '../models/create-order-response';
import { OrderService } from '../services/order.service';

const notWhitespaceValidator: ValidatorFn = control => {
  const value: unknown = control.value;
  return typeof value === 'string' && value.trim().length > 0
    ? null
    : { whitespace: true };
};

const paymentMethodValidator: ValidatorFn = control => {
  const value: unknown = control.value;
  return value === GuestPaymentMethod.CashOnDelivery || value === GuestPaymentMethod.OnlineCard
    ? null
    : { paymentMethod: true };
};

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
  selector: 'app-registered-checkout',
  standalone: true,
  imports: [DecimalPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './registered-checkout.html',
  styleUrl: './registered-checkout.scss'
})
export class RegisteredCheckout {
  private readonly formBuilder = inject(FormBuilder);
  private readonly locationService = inject(LocationService);
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);

  protected readonly cartStore = inject(CartStore);
  protected readonly paymentMethods = GuestPaymentMethod;
  protected readonly selectedAddress = signal<LocationSuggestion | null>(null);
  protected readonly addressSuggestions = signal<LocationSuggestion[]>([]);
  protected readonly addressSuggestionsVisible = signal(false);
  protected readonly isAddressSearching = signal(false);
  protected readonly addressSearchError = signal('');
  protected readonly hasCompletedAddressSearch = signal(false);
  protected readonly submitted = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly submitError = signal('');

  protected readonly checkoutForm = this.formBuilder.group({
    addressSearch: this.formBuilder.nonNullable.control('', [
      Validators.required,
      notWhitespaceValidator,
      Validators.minLength(3)
    ]),
    deliveryAddress: this.formBuilder.nonNullable.control('', Validators.required),
    deliveryLatitude: this.formBuilder.control<number | null>(null, [
      Validators.required,
      Validators.min(-90),
      Validators.max(90)
    ]),
    deliveryLongitude: this.formBuilder.control<number | null>(null, [
      Validators.required,
      Validators.min(-180),
      Validators.max(180)
    ]),
    paymentMethod: this.formBuilder.nonNullable.control(
      GuestPaymentMethod.CashOnDelivery,
      [Validators.required, paymentMethodValidator]
    )
  });

  protected shouldShowError(control: AbstractControl<unknown>): boolean {
    return control.invalid && (control.touched || this.submitted());
  }

  protected shouldShowAddressSelectionError(): boolean {
    return this.selectedAddress() === null
      && this.checkoutForm.controls.addressSearch.value.trim().length > 0
      && (this.checkoutForm.controls.addressSearch.touched || this.submitted());
  }

  protected selectAddress(suggestion: LocationSuggestion): void {
    this.selectedAddress.set(suggestion);
    this.checkoutForm.patchValue({
      addressSearch: suggestion.displayName,
      deliveryAddress: suggestion.displayName,
      deliveryLatitude: suggestion.latitude,
      deliveryLongitude: suggestion.longitude
    }, { emitEvent: false });
    this.checkoutForm.controls.addressSearch.updateValueAndValidity({ emitEvent: false });
    this.addressSuggestionsVisible.set(false);
    this.addressSuggestions.set([]);
    this.hasCompletedAddressSearch.set(false);
    this.addressSearchError.set('');
  }

  protected addressTextChanged(event: Event): void {
    const input = event.target;
    const value = input instanceof HTMLInputElement
      ? input.value
      : this.checkoutForm.controls.addressSearch.value;
    const selectedAddress = this.selectedAddress();
    if (selectedAddress !== null && value !== selectedAddress.displayName) {
      this.clearSelectedAddress();
    }

    this.addressSuggestionsVisible.set(false);
    this.addressSuggestions.set([]);
    this.hasCompletedAddressSearch.set(false);
    this.addressSearchError.set('');
  }

  protected searchAddress(event?: Event): void {
    event?.preventDefault();
    if (this.isAddressSearching()) {
      return;
    }

    const query = this.checkoutForm.controls.addressSearch.value.trim();
    this.addressSearchError.set('');
    this.addressSuggestions.set([]);
    this.hasCompletedAddressSearch.set(false);
    if (query.length < 3) {
      this.checkoutForm.controls.addressSearch.markAsTouched();
      this.addressSuggestionsVisible.set(false);
      return;
    }

    this.isAddressSearching.set(true);
    this.addressSuggestionsVisible.set(true);
    this.locationService.search(query).pipe(
      take(1),
      finalize(() => this.isAddressSearching.set(false))
    ).subscribe({
      next: suggestions => {
        if (this.checkoutForm.controls.addressSearch.value.trim() !== query) {
          return;
        }
        this.addressSuggestions.set(suggestions);
        this.hasCompletedAddressSearch.set(true);
      },
      error: error => {
        if (this.checkoutForm.controls.addressSearch.value.trim() !== query) {
          return;
        }
        this.addressSearchError.set(getApiErrorMessage(
          error,
          'Address lookup failed. Please try again.'
        ));
        this.hasCompletedAddressSearch.set(true);
      }
    });
  }

  protected placeOrder(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.submitted.set(true);
    this.submitError.set('');
    this.checkoutForm.markAllAsTouched();
    if (this.cartStore.isEmpty()) {
      this.submitError.set('Your cart is empty. Add an item before checking out.');
      return;
    }

    if (this.selectedAddress() === null) {
      this.checkoutForm.controls.addressSearch.setErrors({
        ...this.checkoutForm.controls.addressSearch.errors,
        addressSelectionRequired: true
      });
    }
    if (this.checkoutForm.invalid) {
      return;
    }

    if (this.cartStore.distinctItemCount() > MAX_ORDER_LINE_COUNT) {
      this.submitError.set(`An order can contain at most ${MAX_ORDER_LINE_COUNT} different items.`);
      return;
    }
    if (this.cartStore.totalQuantity() > MAX_ORDER_TOTAL_QUANTITY) {
      this.submitError.set(`An order can contain at most ${MAX_ORDER_TOTAL_QUANTITY} items in total.`);
      return;
    }

    const request = this.buildRequest();
    if (request === null) {
      this.submitError.set('Select a valid delivery address before placing the order.');
      return;
    }

    let paymentWindow: Window | null = null;
    if (request.payOnline) {
      paymentWindow = window.open('', '_blank');
      if (paymentWindow === null || paymentWindow.closed) {
        this.submitError.set(
          'The payment tab was blocked. Allow pop-ups for this site, then place the order again.'
        );
        return;
      }
      paymentWindow.opener = null;
    }

    this.isSubmitting.set(true);
    this.orderService.createOrder(request).pipe(
      take(1),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: response => this.handleSuccess(response, request.payOnline, paymentWindow),
      error: error => {
        this.closePaymentWindow(paymentWindow);
        this.submitError.set(getApiErrorMessage(
          error,
          'The order could not be created. Your cart has been preserved.'
        ));
      }
    });
  }

  private clearSelectedAddress(): void {
    this.selectedAddress.set(null);
    this.checkoutForm.patchValue({
      deliveryAddress: '',
      deliveryLatitude: null,
      deliveryLongitude: null
    }, { emitEvent: false });
  }

  private buildRequest(): CreateOrderRequest | null {
    const address = this.selectedAddress();
    if (address === null) {
      return null;
    }

    return {
      deliveryAddress: address.displayName,
      deliveryLatitude: address.latitude,
      deliveryLongitude: address.longitude,
      payOnline: this.checkoutForm.controls.paymentMethod.value === GuestPaymentMethod.OnlineCard,
      items: this.cartStore.items().map(item => ({
        menuItemId: item.menuItemId,
        quantity: item.quantity
      }))
    };
  }

  private handleSuccess(
    response: CreateOrderResponse,
    payOnline: boolean,
    paymentWindow: Window | null
  ): void {
    this.cartStore.clear();
    if (!payOnline) {
      void this.router.navigate(['/orders', response.orderId]);
      return;
    }

    if (paymentWindow === null || paymentWindow.closed) {
      void this.router.navigate(['/orders', response.orderId], {
        queryParams: { paymentIssue: 'closed' }
      });
      return;
    }

    if (!isValidPaymentUrl(response.paymentUrl)) {
      this.closePaymentWindow(paymentWindow);
      void this.router.navigate(['/orders', response.orderId], {
        queryParams: { paymentIssue: 'missing' }
      });
      return;
    }

    try {
      paymentWindow.opener = null;
      paymentWindow.location.href = response.paymentUrl;
      void this.router.navigate(['/orders', response.orderId], {
        queryParams: { paymentOpened: true }
      });
    } catch {
      this.closePaymentWindow(paymentWindow);
      void this.router.navigate(['/orders', response.orderId], {
        queryParams: { paymentIssue: 'open' }
      });
    }
  }

  private closePaymentWindow(paymentWindow: Window | null): void {
    if (paymentWindow !== null && !paymentWindow.closed) {
      paymentWindow.close();
    }
  }
}
