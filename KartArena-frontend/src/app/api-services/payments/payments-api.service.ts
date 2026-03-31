import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListPaymentsRequest,
  ListPaymentsResponse,
  GetPaymentByIdQueryDto,
  CreatePaymentCommand,
  UpdatePaymentCommand,
} from './payments-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({ providedIn: 'root' })
export class PaymentsApiService {
  private readonly baseUrl = `${environment.apiUrl}/payments/controller`;
  private readonly http = inject(HttpClient);

  list(request?: ListPaymentsRequest): Observable<ListPaymentsResponse> {
    const params = request ? buildHttpParams(request) : undefined;
    return this.http.get<ListPaymentsResponse>(this.baseUrl, { params });
  }

  getById(id: number): Observable<GetPaymentByIdQueryDto> {
    return this.http.get<GetPaymentByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreatePaymentCommand): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.baseUrl, payload);
  }

  update(id: number, payload: UpdatePaymentCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}