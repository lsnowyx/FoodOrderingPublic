import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthStore } from '../../auth/services/auth-store.service';
import { CartItem, MAX_CART_ITEM_QUANTITY } from '../models/cart-item';
import { CartStore } from '../services/cart-store.service';

@Component({
  selector: 'app-cart-page',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss'
})
export class CartPage {
  protected readonly cartStore = inject(CartStore);
  protected readonly authStore = inject(AuthStore);
  protected readonly maxItemQuantity = MAX_CART_ITEM_QUANTITY;

  protected increaseQuantity(item: CartItem): void {
    this.cartStore.increaseQuantity(item.menuItemId);
  }

  protected decreaseQuantity(item: CartItem): void {
    this.cartStore.decreaseQuantity(item.menuItemId);
  }

  protected removeItem(item: CartItem): void {
    this.cartStore.removeItem(item.menuItemId);
  }

  protected clearCart(): void {
    this.cartStore.clear();
  }
}
