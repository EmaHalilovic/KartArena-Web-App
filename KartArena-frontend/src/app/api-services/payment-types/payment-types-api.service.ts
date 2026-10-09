import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListPaymentTypesRequest,
  ListPaymentTypesResponse,
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
    return this.http.get<ListPaymentTypesResponse>(this.baseUrl, { params });
  }

  getById(id: number): Observable<GetPaymentTypeByIdQueryDto> {
    return this.http.get<GetPaymentTypeByIdQueryDto>(`${this.baseUrl}/${id}`);
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

 
}
