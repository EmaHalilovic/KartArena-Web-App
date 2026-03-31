import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  CreateEquipmentTypeCommand,
  EquipmentCategory,
  EquipmentItemStatus,
  EquipmentStockStatus,
  GetEquipmentByIdQueryDto,
  GetEquipmentByIdQueryDtoItem,
  ListEquipmentQueryDto,
  ListEquipmentRequest,
  ListEquipmentResponse,
  ListWithItemsEquipmentQueryDto,
  ListWithItemsEquipmentQueryDtoItem,
  ListWithItemsEquipmentRequest,
  ListWithItemsEquipmentResponse,
  UpdateEquipmentTypeCommand,
} from './equipment-api.models';

@Injectable({ providedIn: 'root' })
export class EquipmentApiService {
  private readonly lowStockThreshold = 5;
  private readonly baseUrl = `${environment.apiUrl}/equipment/controller`;
  private readonly http = inject(HttpClient);

   list(request?: ListEquipmentRequest): Observable<ListEquipmentResponse> {
    const params = request ? buildHttpParams(this.mapListRequest(request) as any) : undefined;

    return this.http.get<ListEquipmentResponse>(this.baseUrl, {
      params,
    }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((item) => this.mapEquipmentListItem(item)),
      }))
    );
  }

  /**
   * GET /Orders/with-items
   * List orders with items included.
   * Use this when you need to display order items in the list.
   */
  listWithItems(request?: ListWithItemsEquipmentRequest): Observable<ListWithItemsEquipmentResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListWithItemsEquipmentResponse>(`${this.baseUrl}/with-items`, {
      params,
    }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((item) => this.mapEquipmentWithItems(item)),
      }))
    );
  }

  /**
   * GET /Orders/{id}
   * Get a single order by ID with full details including items.
   */
  getById(id: number): Observable<GetEquipmentByIdQueryDto> {
    return this.http
      .get<GetEquipmentByIdQueryDto>(`${this.baseUrl}/${id}`)
      .pipe(map((equipment) => this.mapEquipmentDetails(equipment)));
  }

  /**
   * POST /Orders
   * Create a new order.
   * @returns ID of the newly created order
   */
  create(payload: CreateEquipmentTypeCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /Orders/{id}
   * Update an existing order.
   * Can update order note and items.
   */
  update(id: number, payload: UpdateEquipmentTypeCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

 disable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/disable`, {});
  }

  /**
   * PUT /ProductCategories/{id}/enable
   * Enable a category.
   */
  enable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/enable`, {});
  }
  /**
   * PUT /Orders/{id}/change-status
   * Change order status.
   * Validates status transitions on backend.
   */
   delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private mapListRequest(request: ListEquipmentRequest): ListEquipmentRequest {
    return {
      ...request,
      category: request.category == null ? null : this.toCategoryCode(request.category),
      stockStatus: request.stockStatus == null ? null : this.toStockStatusCode(request.stockStatus),
    } as ListEquipmentRequest;
  }

  private mapEquipmentListItem(item: ListEquipmentQueryDto): ListEquipmentQueryDto {
    return {
      ...item,
      category: this.normalizeCategory(item.category),
      stockStatus: this.normalizeStockStatus(item.stockStatus),
      isActive: this.normalizeBoolean(item.isActive),
    };
  }

  private mapEquipmentWithItems(item: ListWithItemsEquipmentQueryDto): ListWithItemsEquipmentQueryDto {
    return {
      ...item,
      category: this.normalizeCategory(item.category),
      stockStatus: this.normalizeStockStatus(item.stockStatus),
      items: item.items.map((equipmentItem) => this.normalizeEquipmentListItemStatus(equipmentItem)),
    };
  }

  private mapEquipmentDetails(item: GetEquipmentByIdQueryDto): GetEquipmentByIdQueryDto {
    return {
      ...item,
      category: this.normalizeCategory(item.category),
      stockStatus: this.normalizeStockStatus(item.stockStatus),
      isActive: this.normalizeBoolean(item.isActive),
      items: item.items.map((equipmentItem) => this.normalizeEquipmentDetailItemStatus(equipmentItem)),
    };
  }

  private normalizeEquipmentListItemStatus(
    item: ListWithItemsEquipmentQueryDtoItem
  ): ListWithItemsEquipmentQueryDtoItem {
    return {
      ...item,
      status: this.normalizeItemStatus(item.status),
    };
  }

  private normalizeEquipmentDetailItemStatus(item: GetEquipmentByIdQueryDtoItem): GetEquipmentByIdQueryDtoItem {
    return {
      ...item,
      status: this.normalizeItemStatus(item.status),
    };
  }

  private normalizeCategory(category: unknown): EquipmentCategory {
    if (category === EquipmentCategory.Helmet || category === 0) {
      return EquipmentCategory.Helmet;
    }

    if (category === EquipmentCategory.Suit || category === 1) {
      return EquipmentCategory.Suit;
    }

    if (category === EquipmentCategory.Gloves || category === 2) {
      return EquipmentCategory.Gloves;
    }

    if (category === EquipmentCategory.Balaclava || category === 3) {
      return EquipmentCategory.Balaclava;
    }

    if (category === EquipmentCategory.Other || category === 4) {
      return EquipmentCategory.Other;
    }

    const normalized = String(category ?? '')
      .replace(/[^a-z]/gi, '')
      .toLowerCase();

    switch (normalized) {
      case 'helmet':
        return EquipmentCategory.Helmet;
      case 'suit':
      case 'racesuit':
        return EquipmentCategory.Suit;
      case 'gloves':
      case 'glove':
        return EquipmentCategory.Gloves;
      case 'balaclava':
        return EquipmentCategory.Balaclava;
      case 'other':
        return EquipmentCategory.Other;
      default:
        return EquipmentCategory.Other;
    }
  }

  private toCategoryCode(category: EquipmentCategory): number {
    switch (category) {
      case EquipmentCategory.Helmet:
        return 0;
      case EquipmentCategory.Suit:
        return 1;
      case EquipmentCategory.Gloves:
        return 2;
      case EquipmentCategory.Balaclava:
        return 3;
      case EquipmentCategory.Other:
      default:
        return 4;
    }
  }

  private normalizeStockStatus(status: unknown): EquipmentStockStatus {
    if (status === EquipmentStockStatus.Good || status === 0) {
      return EquipmentStockStatus.Good;
    }

    if (status === EquipmentStockStatus.Low || status === 1) {
      return EquipmentStockStatus.Low;
    }

    if (status === EquipmentStockStatus.OutOfStock || status === 2) {
      return EquipmentStockStatus.OutOfStock;
    }

    const normalized = String(status ?? '')
      .replace(/[^a-z]/gi, '')
      .toLowerCase();

    switch (normalized) {
      case 'good':
      case 'instock':
      case 'available':
        return EquipmentStockStatus.Good;
      case 'low':
      case 'lowstock':
        return EquipmentStockStatus.Low;
      case 'outofstock':
      case 'outstock':
      case 'unavailable':
        return EquipmentStockStatus.OutOfStock;
      default:
        return EquipmentStockStatus.OutOfStock;
    }
  }

  private toStockStatusCode(status: EquipmentStockStatus): number {
    switch (status) {
      case EquipmentStockStatus.Good:
        return 0;
      case EquipmentStockStatus.Low:
        return 1;
      case EquipmentStockStatus.OutOfStock:
      default:
        return 2;
    }
  }

  private normalizeItemStatus(status: unknown): EquipmentItemStatus {
    if (status === EquipmentItemStatus.Available || status === 0) {
      return EquipmentItemStatus.Available;
    }

    if (status === EquipmentItemStatus.InUse || status === 1) {
      return EquipmentItemStatus.InUse;
    }

    if (status === EquipmentItemStatus.Maintenance || status === 2) {
      return EquipmentItemStatus.Maintenance;
    }

    if (status === EquipmentItemStatus.Lost || status === 3) {
      return EquipmentItemStatus.Lost;
    }

    const normalized = String(status ?? '')
      .replace(/[^a-z]/gi, '')
      .toLowerCase();

    switch (normalized) {
      case 'available':
      case 'instock':
        return EquipmentItemStatus.Available;
      case 'inuse':
      case 'inservice':
        return EquipmentItemStatus.InUse;
      case 'maintenance':
      case 'repair':
        return EquipmentItemStatus.Maintenance;
      case 'lost':
      case 'missing':
        return EquipmentItemStatus.Lost;
      default:
        return EquipmentItemStatus.Available;
    }
  }

  private normalizeBoolean(value: unknown): boolean {
    if (typeof value === 'boolean') {
      return value;
    }

    if (typeof value === 'number') {
      return value !== 0;
    }

    const normalized = String(value ?? '').trim().toLowerCase();

    if (normalized === 'true' || normalized === '1') {
      return true;
    }

    if (normalized === 'false' || normalized === '0') {
      return false;
    }

    return Boolean(value);
  }
}
