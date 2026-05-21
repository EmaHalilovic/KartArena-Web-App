import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListPaymentTypesRequest,
  ListPaymentTypesResponse,
  ListPaymentTypesQueryDto,
  GetPaymentTypeByIdQueryDto,
  CreatePaymentTypeCommand,
  UpdatePaymentTypeCommand,
} from './payment-types-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({ providedIn: 'root' })
export class PaymentTypesApiService {
  private readonly baseUrl = `${environment.apiUrl}/payment-types/controller`;
  private readonly http = inject(HttpClient);

  list(request?: ListPaymentTypesRequest): Observable<ListPaymentTypesResponse> {
    const params = request ? buildHttpParams(request) : undefined;
    return this.http.get<ListPaymentTypesResponse>(this.baseUrl, { params }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((item) => this.mapListItem(item)),
      }))
    );
  }

  getById(id: number): Observable<GetPaymentTypeByIdQueryDto> {
    return this.http.get<GetPaymentTypeByIdQueryDto>(`${this.baseUrl}/${id}`).pipe(
      map((item) => this.mapDetails(item))
    );
  }

  create(payload: CreatePaymentTypeCommand): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.baseUrl, payload);
  }

  update(id: number, payload: UpdatePaymentTypeCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  enable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/enable`, {});
  }

  disable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/disable`, {});
  }

  private mapListItem(item: ListPaymentTypesQueryDto): ListPaymentTypesQueryDto {
    return {
      ...item,
      allowedOnline: this.normalizeBoolean(item.allowedOnline),
      allowedAtDesk: this.normalizeBoolean(item.allowedAtDesk),
      isEnabled: this.normalizeBoolean(item.isEnabled),
    };
  }

  private mapDetails(item: GetPaymentTypeByIdQueryDto): GetPaymentTypeByIdQueryDto {
    return {
      ...item,
      allowedOnline: this.normalizeBoolean(item.allowedOnline),
      allowedAtDesk: this.normalizeBoolean(item.allowedAtDesk),
      isEnabled: this.normalizeBoolean(item.isEnabled),
    };
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
