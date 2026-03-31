import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListReservationRequest,
  ListReservationResponse,
  GetReservationByIdQueryDto,
  CreateReservationCommand,
  UpdateReservationCommand,
} from './reservation-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({ providedIn: 'root' })
export class ReservationApiService {
  private readonly baseUrl = `${environment.apiUrl}/reservation/controller`;
  private http = inject(HttpClient);

  list(request?: ListReservationRequest): Observable<ListReservationResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListReservationResponse>(this.baseUrl, { params });
  }

  getById(id: number): Observable<GetReservationByIdQueryDto> {
    return this.http.get<GetReservationByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreateReservationCommand): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload);
  }


  update(id: number, payload: UpdateReservationCommand): Observable<number> {
    // kod tebe equipment vraća void, ali UpdateReservationHandler vraća int (Id)
    // ako endpoint vraća void, promijeni Observable<number> u Observable<void>
    return this.http.put<number>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
