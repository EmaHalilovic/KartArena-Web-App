import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  GetKartByIdQueryDto,
  ListKartsQueryDto,
  ListKartsRequest,
  PageResult,
  CreateKartCommand,
  UpdateKartCommand,
} from './karts-api.models';

@Injectable({ providedIn: 'root' })
export class KartsApiService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/karts`;

  list(request: ListKartsRequest): Observable<PageResult<ListKartsQueryDto>> {
    const params = buildHttpParams(request);
    return this.http.get<any>(this.baseUrl, { params }).pipe(
      map((res) => {
        // Backend-i znaju vraćati razne oblike odgovora:
        // 1) Paged: { items, totalItems } ili { Items, Total }
        // 2) Wrapped: { data: {...} } ili { result: {...} }
        // 3) Plain list: [ ... ]
        const payload = res?.data ?? res?.result ?? res?.value ?? res;

        const isArray = Array.isArray(payload);
        const items = (isArray ? payload : (payload?.items ?? payload?.Items)) ?? [];

        const total =
          (isArray ? (items as any[])?.length : undefined) ??
          payload?.totalItems ??
          payload?.TotalItems ??
          payload?.total ??
          payload?.Total ??
          payload?.count ??
          payload?.Count ??
          0;

        const pageSize = request?.paging?.pageSize ?? 10;
        const currentPage = request?.paging?.page ?? 1;
        const totalPages = pageSize > 0 ? Math.ceil(total / pageSize) : 1;

        return {
          items,
          pageSize,
          currentPage,
          includedTotal: true,
          totalItems: total,
          totalPages,
        } as PageResult<ListKartsQueryDto>;
      })
    );
  }

  getById(id: number): Observable<GetKartByIdQueryDto> {
    return this.http.get<GetKartByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  create(command: CreateKartCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, command);
  }

  update(id: number, command: UpdateKartCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, command);
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
