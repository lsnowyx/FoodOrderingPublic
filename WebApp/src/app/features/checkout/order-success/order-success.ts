import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AuthStore } from '../../auth/services/auth-store.service';
import { GuestOrderStorage } from '../services/guest-order-storage.service';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function readOrderId(value: string | null): string | null {
  if (value === null) {
    return null;
  }

  const orderId = value.trim();
  return GUID_PATTERN.test(orderId) ? orderId : null;
}

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './order-success.html',
  styleUrl: './order-success.scss'
})
export class OrderSuccess {
  private readonly route = inject(ActivatedRoute);
  private readonly guestOrderStorage = inject(GuestOrderStorage);
  private readonly authStore = inject(AuthStore);

  protected readonly orderId = readOrderId(
    this.route.snapshot.queryParamMap.get('orderId')
  );
  protected readonly shortOrderNumber = this.orderId?.slice(0, 8).toUpperCase() ?? null;
  protected readonly customerOrderId = computed(() =>
    this.authStore.isAuthenticated() ? this.orderId : null
  );
  protected readonly trackingReference = computed(() => {
    const reference = this.guestOrderStorage.reference();
    if (reference === null || this.orderId === null) {
      return null;
    }

    return reference.orderId.toLowerCase() === this.orderId.toLowerCase()
      ? reference
      : null;
  });
}
