import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { Category } from '../models/category';
import { PagedResponse } from '../../../shared/models/paged-response';
import { MenuItem } from '../models/menu-item';
import { MenuItemDetails } from '../models/menu-item-details';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly catalogUrl = '/api/catalog';

  getCategories(
    page: number,
    pageSize = 12
  ): Observable<PagedResponse<Category>> {
    return this.http.get<PagedResponse<Category>>(
      `${this.catalogUrl}/categories`,
      { params: { page, pageSize } }
    );
  }

  getMenuItemsByCategory(
    categoryId: string,
    page: number,
    pageSize = 12
  ): Observable<PagedResponse<MenuItem>> {
    return this.http.get<PagedResponse<MenuItem>>(
      `${this.catalogUrl}/categories/${categoryId}/menu-items`,
      { params: { page, pageSize } }
    );
  }

  getMenuItemDetails(menuItemId: string): Observable<MenuItemDetails> {
    return this.http.get<MenuItemDetails>(
      `${this.catalogUrl}/menu-items/${menuItemId}`
    );
  }
}
