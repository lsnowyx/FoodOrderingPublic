import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  Observable,
  catchError,
  combineLatest,
  distinctUntilChanged,
  map,
  of,
  startWith,
  switchMap
} from 'rxjs';

import { LoadState } from '../../../shared/models/load-state';
import { PagedResponse } from '../../../shared/models/paged-response';
import { MenuItem } from '../models/menu-item';
import { CatalogService } from '../services/catalog.service';

function readPage(value: string | null): number {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}

@Component({
  selector: 'app-menu-item-list',
  imports: [AsyncPipe, DecimalPipe, RouterLink],
  templateUrl: './menu-item-list.html',
  styleUrl: './menu-item-list.scss'
})
export class MenuItemList {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogService = inject(CatalogService);

  readonly menuItemsState$: Observable<LoadState<PagedResponse<MenuItem>>> =
    combineLatest([this.route.paramMap, this.route.queryParamMap]).pipe(
      map(([routeParameters, queryParameters]) => ({
        categoryId: routeParameters.get('categoryId'),
        page: readPage(queryParameters.get('page'))
      })),
      distinctUntilChanged((previous, current) =>
        previous.categoryId === current.categoryId && previous.page === current.page
      ),
      switchMap(({ categoryId, page }) => {
        if (categoryId === null) {
          return of({
            status: 'error',
            message: 'The category ID is missing from the address.'
          } as const);
        }

        return this.catalogService.getMenuItemsByCategory(categoryId, page).pipe(
          map(data => ({ status: 'success', data }) as const),
          catchError(() => of({
            status: 'error',
            message: 'Menu items could not be loaded. Please try again.'
          } as const)),
          startWith({ status: 'loading' } as const)
        );
      })
    );
}
