import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PdfReportFilter {
  id?: number | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/reports`;

  reservationsPdf(filter: PdfReportFilter): Observable<HttpResponse<Blob>> {
    return this.download('reservations', filter);
  }

  paymentsPdf(filter: PdfReportFilter): Observable<HttpResponse<Blob>> {
    return this.download('payments', filter);
  }

  private download(type: string, filter: PdfReportFilter): Observable<HttpResponse<Blob>> {
    let params = new HttpParams();
    if (filter.id != null) params = params.set('id', filter.id);
    if (filter.dateFrom) params = params.set('dateFrom', filter.dateFrom);
    if (filter.dateTo) params = params.set('dateTo', filter.dateTo);

    return this.http.get(`${this.baseUrl}/${type}/pdf`, {
      params,
      observe: 'response',
      responseType: 'blob',
    });
  }
}
