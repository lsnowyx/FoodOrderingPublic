import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { authGuard } from './features/auth/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home')
        .then(module => module.Home)
  },
  {
    path: 'categories',
    loadComponent: () =>
      import('./features/catalog/category-list/category-list')
        .then(module => module.CategoryList)
  },
  {
    path: 'categories/:categoryId/menu-items',
    loadComponent: () =>
      import('./features/catalog/menu-item-list/menu-item-list')
        .then(module => module.MenuItemList)
  },
  {
    path: 'menu-items/:menuItemId',
    loadComponent: () =>
      import('./features/catalog/menu-item-details/menu-item-details')
        .then(module => module.MenuItemDetails)
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./features/cart/cart-page/cart-page')
        .then(module => module.CartPage)
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(module => module.Login)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register')
        .then(module => module.Register)
  },
  {
    path: 'checkout',
    loadComponent: () =>
      import('./features/checkout/guest-checkout/guest-checkout')
        .then(module => module.GuestCheckout)
  },
  {
    path: 'checkout/customer',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/orders/registered-checkout/registered-checkout')
        .then(module => module.RegisteredCheckout)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/orders/order-list/order-list')
        .then(module => module.OrderList)
  },
  {
    path: 'orders/:orderId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/orders/order-details/order-details')
        .then(module => module.OrderDetails)
  },
  {
    path: 'orders/:orderId/tracking',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/orders/customer-tracking/customer-tracking')
        .then(module => module.CustomerTracking)
  },
  {
    path: 'payment/success',
    loadComponent: () =>
      import('./features/checkout/payment-success/payment-success')
        .then(module => module.PaymentSuccess)
  },
  {
    path: 'order-success',
    loadComponent: () =>
      import('./features/checkout/order-success/order-success')
        .then(module => module.OrderSuccess)
  },
  {
    path: 'order-cancelled',
    loadComponent: () =>
      import('./features/checkout/order-cancelled/order-cancelled')
        .then(module => module.OrderCancelled)
  },
  {
    path: 'payment/cancel',
    loadComponent: () =>
      import('./features/checkout/payment-cancel/payment-cancel')
        .then(module => module.PaymentCancel)
  },
  {
    path: 'track',
    loadComponent: () =>
      import('./features/tracking/tracking-page/tracking-page')
        .then(module => module.TrackingPage)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
