import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { CartStore } from './features/cart/services/cart-store.service';
import { AuthStore } from './features/auth/services/auth-store.service';
import { DeliverySignalRService } from './features/tracking/services/delivery-signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  private readonly router = inject(Router);
  private readonly deliverySignalR = inject(DeliverySignalRService);

  protected readonly cartStore = inject(CartStore);
  protected readonly authStore = inject(AuthStore);
  protected readonly title = signal('WebApp');

  protected logout(): void {
    this.authStore.logout();
    void this.deliverySignalR.stopConnection();
    void this.router.navigate(['/']);
  }
}
