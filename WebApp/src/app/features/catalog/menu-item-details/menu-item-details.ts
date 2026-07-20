import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, catchError, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';

import { LoadState } from '../../../shared/models/load-state';
import { CartStore } from '../../cart/services/cart-store.service';
import { MenuItemDetails as MenuItemDetailsModel } from '../models/menu-item-details';
import { CatalogService } from '../services/catalog.service';

@Component({
  selector: 'app-menu-item-details',
  imports: [AsyncPipe, DecimalPipe, RouterLink],
  templateUrl: './menu-item-details.html',
  styleUrl: './menu-item-details.scss'
})
export class MenuItemDetails {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogService = inject(CatalogService);
  private readonly cartStore = inject(CartStore);

  protected readonly cartFeedback = signal('');

  readonly menuItemState$: Observable<LoadState<MenuItemDetailsModel>> =
    this.route.paramMap.pipe(
      map(parameters => parameters.get('menuItemId')),
      distinctUntilChanged(),
      switchMap(menuItemId => {
        if (menuItemId === null) {
          return of({
            status: 'error',
            message: 'The menu item ID is missing from the address.'
          } as const);
        }

        return this.catalogService.getMenuItemDetails(menuItemId).pipe(
          map(data => ({ status: 'success', data }) as const),
          catchError(() => of({
            status: 'error',
            message: 'Menu-item details could not be loaded. Please try again.'
          } as const)),
          startWith({ status: 'loading' } as const)
        );
      })
    );

  protected addToCart(menuItem: MenuItemDetailsModel): void {
    const added = this.cartStore.addItem({
      menuItemId: menuItem.id,
      name: menuItem.name,
      imageUrl: menuItem.pictures[0]?.imageUrl ?? null,
      unitPrice: menuItem.price,
      caloriesPerUnit: menuItem.totalCalories,
      quantity: 1
    });

    this.cartFeedback.set(
      added ? 'Added to cart.' : 'Maximum quantity reached.'
    );
  }
}
