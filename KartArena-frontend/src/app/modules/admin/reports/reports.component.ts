import { Component, inject } from '@angular/core';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { finalize, Observable } from 'rxjs';
import { PdfReportFilter, ReportsApiService } from '../../../api-services/reports/reports-api.service';
import { ToasterService } from '../../../core/services/toaster.service';

type ReportType = 'reservations' | 'payments';

@Component({
  selector: 'app-admin-reports',
  standalone: false,
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss',
})
export class ReportsComponent {
  private readonly api = inject(ReportsApiService);
  private readonly toaster = inject(ToasterService);
  private readonly translate = inject(TranslateService);

  readonly reportTypes: ReportType[] = ['reservations', 'payments'];
  filters: Record<ReportType, PdfReportFilter> = { reservations: {}, payments: {} };
  loading: Record<ReportType, boolean> = { reservations: false, payments: false };

  generate(type: ReportType): void {
    const filter = this.filters[type];
    if (filter.dateFrom && filter.dateTo && filter.dateFrom > filter.dateTo) {
      this.toaster.error(this.translate.instant('REPORTS.MESSAGES.INVALID_RANGE'));
      return;
    }

    this.loading[type] = true;
    const request$: Observable<HttpResponse<Blob>> = type === 'reservations'
      ? this.api.reservationsPdf(filter)
      : this.api.paymentsPdf(filter);

    request$.pipe(finalize(() => (this.loading[type] = false))).subscribe({
      next: (response) => {
        if (!response.body) return;
        this.save(response.body, this.filename(response, type));
        this.toaster.success(this.translate.instant('REPORTS.MESSAGES.SUCCESS'));
      },
      error: (error: HttpErrorResponse) => {
        const key = error.status === 404 ? 'REPORTS.MESSAGES.EMPTY' :
          error.status === 400 ? 'REPORTS.MESSAGES.INVALID_FILTERS' : 'REPORTS.MESSAGES.ERROR';
        this.toaster.error(this.translate.instant(key));
      },
    });
  }

  private filename(response: HttpResponse<Blob>, type: ReportType): string {
    const disposition = response.headers.get('content-disposition') ?? '';
    const utf8 = disposition.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
    const basic = disposition.match(/filename="?([^";]+)"?/i)?.[1];
    return decodeURIComponent(utf8 ?? basic ?? `${type}-report.pdf`);
  }

  private save(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
