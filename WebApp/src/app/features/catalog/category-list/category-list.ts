import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, catchError, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';

import { LoadState } from '../../../shared/models/load-state';
import { PagedResponse } from '../../../shared/models/paged-response';
import { Category } from '../models/category';
import { CatalogService } from '../services/catalog.service';

function readPage(value: string | null): number {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}

@Component({
  selector: 'app-category-list',
  imports: [AsyncPipe, RouterLink],
  templateUrl: './category-list.html',
  styleUrl: './category-list.scss',
})
export class CategoryList {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogService = inject(CatalogService);

  readonly categoriesState$: Observable<LoadState<PagedResponse<Category>>> =
    this.route.queryParamMap.pipe(
      map(parameters => readPage(parameters.get('page'))),
      distinctUntilChanged(),
      switchMap(page =>
        this.catalogService.getCategories(page).pipe(
          map(data => ({ status: 'success', data }) as const),
          catchError(() => of({
            status: 'error',
            message: 'Categories could not be loaded. Please try again.'
          } as const)),
          startWith({ status: 'loading' } as const)
        )
      )
    );
}
