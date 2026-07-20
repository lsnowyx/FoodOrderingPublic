import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AuthStore } from '../../auth/services/auth-store.service';
import { GuestOrderStorage } from '../services/guest-order-storage.service';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

@Component({
  selector: 'app-payment-cancel',
  imports: [RouterLink],
  templateUrl: './payment-cancel.html',
  styleUrl: './payment-cancel.scss'
})
export class PaymentCancel {
  private readonly route = inject(ActivatedRoute);
  private readonly authStore = inject(AuthStore);
  private readonly guestOrderStorage = inject(GuestOrderStorage);

  private readonly orderId = this.route.snapshot.queryParamMap.get('orderId');
  protected readonly guestOrderReference = computed(() => {
    const reference = this.guestOrderStorage.reference();
    if (reference === null || this.orderId === null) {
      return this.orderId === null ? reference : null;
    }
    return reference.orderId.toLowerCase() === this.orderId.toLowerCase()
      ? reference
      : null;
  });
  protected readonly customerOrderId = computed(() => {
    return this.authStore.isAuthenticated()
      && this.orderId !== null
      && GUID_PATTERN.test(this.orderId)
      ? this.orderId
      : null;
  });
}
