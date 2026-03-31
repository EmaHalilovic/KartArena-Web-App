import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  CreateEquipmentItemCommand,
  EquipmentItemStatus,
  GetEquipmentItemByIdQueryDto,
  ListEquipmentItemQueryDto,
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
    const params = request ? buildHttpParams(this.mapListRequest(request) as any) : undefined;

    return this.http.get<ListEquipmentItemResponse>(this.baseUrl, { params }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((item) => this.mapEquipmentItem(item)),
      }))
    );
  }

  /**
   * GET /equipment-item/controller/{id}
   * Get a single equipment item by ID.
   */
  getById(id: number): Observable<GetEquipmentItemByIdQueryDto> {
    return this.http
      .get<GetEquipmentItemByIdQueryDto>(`${this.baseUrl}/${id}`)
      .pipe(map((item) => this.mapEquipmentItemDetails(item)));
  }

  /**
   * POST /equipment-item/controller
   * Create a new equipment item.
   * @returns ID of the newly created equipment item
   */
  create(payload: CreateEquipmentItemCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, this.mapCreateCommand(payload));
  }

  /**
   * PUT /equipment-item/controller/{id}
   * Update an existing equipment item.
   */
  update(id: number, payload: UpdateEquipmentItemCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, this.mapUpdateCommand(payload));
  }

  /**
   * DELETE /equipment-item/controller/{id}
   * Delete an equipment item.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private mapListRequest(request: ListEquipmentItemRequest): ListEquipmentItemRequest {
    return {
      ...request,
      status: request.status == null ? null : this.toStatusCode(request.status),
    } as ListEquipmentItemRequest;
  }

  private mapEquipmentItem(item: ListEquipmentItemQueryDto): ListEquipmentItemQueryDto {
    return {
      ...item,
      status: this.normalizeItemStatus(item.status),
    };
  }

  private mapEquipmentItemDetails(item: GetEquipmentItemByIdQueryDto): GetEquipmentItemByIdQueryDto {
    return {
      ...item,
      status: this.normalizeItemStatus(item.status),
    };
  }

  private mapCreateCommand(payload: CreateEquipmentItemCommand) {
    return {
      ...payload,
      status: this.toStatusCode(payload.status),
    };
  }

  private mapUpdateCommand(payload: UpdateEquipmentItemCommand) {
    return {
      ...payload,
      status: payload.status == null ? payload.status : this.toStatusCode(payload.status),
    };
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

  private toStatusCode(status: EquipmentItemStatus): number {
    switch (status) {
      case EquipmentItemStatus.Available:
        return 0;
      case EquipmentItemStatus.InUse:
        return 1;
      case EquipmentItemStatus.Maintenance:
        return 2;
      case EquipmentItemStatus.Lost:
      default:
        return 3;
    }
  }
}
