import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  CreateEquipmentTypeCommand,
  GetEquipmentByIdQueryDto,
  ListEquipmentRequest,
  ListEquipmentResponse,
  ListWithItemsEquipmentRequest,
  ListWithItemsEquipmentResponse,
  UpdateEquipmentTypeCommand,
} from './equipment-api.models';

@Injectable({ providedIn: 'root' })
export class EquipmentApiService {
  private readonly baseUrl = `${environment.apiUrl}/equipment/controller`;
  private readonly http = inject(HttpClient);

   list(request?: ListEquipmentRequest): Observable<ListEquipmentResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListEquipmentResponse>(this.baseUrl, {params});
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
    });
  }

  /**
   * GET /Orders/{id}
   * Get a single order by ID with full details including items.
   */
  getById(id: number): Observable<GetEquipmentByIdQueryDto> {
    return this.http.get<GetEquipmentByIdQueryDto>(`${this.baseUrl}/${id}`);
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

  

  
}
