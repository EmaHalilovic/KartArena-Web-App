import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  CreateEquipmentItemCommand,
  GetEquipmentItemByIdQueryDto,
  ListEquipmentItemRequest,
  ListEquipmentItemResponse,
  UpdateEquipmentItemCommand,
} from './equipmentItem-api.models';

@Injectable({ providedIn: 'root' })
export class EquipmentItemApiService {
  private readonly baseUrl = `${environment.apiUrl}/equipmentItems/controller`;
  private readonly http = inject(HttpClient);

  /**
   * GET /equipment-item/controller
   * List equipment items.
   */
  list(request?: ListEquipmentItemRequest): Observable<ListEquipmentItemResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListEquipmentItemResponse>(this.baseUrl, { params });
  }

  /**
   * GET /equipment-item/controller/{id}
   * Get a single equipment item by ID.
   */
  getById(id: number): Observable<GetEquipmentItemByIdQueryDto> {
    return this.http
      .get<GetEquipmentItemByIdQueryDto>(`${this.baseUrl}/${id}`);
      
  }

  /**
   * POST /equipment-item/controller
   * Create a new equipment item.
   * @returns ID of the newly created equipment item
   */
  create(payload: CreateEquipmentItemCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /equipment-item/controller/{id}
   * Update an existing equipment item.
   */
  update(id: number, payload: UpdateEquipmentItemCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /**
   * DELETE /equipment-item/controller/{id}
   * Delete an equipment item.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

 
}
