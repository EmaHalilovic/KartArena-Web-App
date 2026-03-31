import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { finalize } from 'rxjs/operators';

import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';
import {
  ListReservationRequest,
  ListReservationQueryDto,
  ListReservationResponse,
} from '../../../api-services/reservations/reservation-api.models';

import { ConfirmDeleteDialogComponent } from '../reservation/dialogs/confirm-delete/confirm-delete-dialog.component';

@Component({
  selector: 'app-reservation',
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.scss'],
  standalone: false,
})
export class ReservationComponent implements OnInit {
  private api = inject(ReservationApiService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  isLoading = false;
  errorMessage: string | null = null;

  // ✅ HTML koristi [dataSource]="data"
  data: ListReservationQueryDto[] = [];

  // ✅ za <tr mat-header-row ...>
  displayedColumns: string[] = ['id', 'userId', 'trackId', 'kartId', 'date', 'startTime', 'endTime', 'actions'];

  // paging (ako kasnije dodaš paginator)
  page = 1;
  pageSize = 10;
  totalCount = 0;

  search: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.errorMessage = null;

    const req: ListReservationRequest = {
      paging: { page: this.page, pageSize: this.pageSize },
      search: this.search,
    };

    this.api
      .list(req)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res: ListReservationResponse) => {
          // prilagodi ako se PageResult zove drugačije
          const items = (res as any).items ?? (res as any).data ?? [];
          this.data = items as ListReservationQueryDto[];

          this.totalCount = (res as any).totalCount ?? (res as any).total ?? 0;
        },
        error: (err) => {
          console.error('Load reservations error:', err);
          this.errorMessage =
            err?.error?.message ??
            err?.message ??
            'Greška prilikom učitavanja rezervacija.';
        },
      });
  }

  // ✅ HTML: (click)="onAdd()"
  onAdd(): void {
    this.router.navigate(['/client/reservation/add']);
  }

  // ✅ HTML: (click)="onEdit(r.id!)"
  onEdit(id: number): void {
    this.router.navigate(['/client/reservation/edit', id]);
  }

  // ✅ HTML: (click)="onDelete(r.id!)"
  onDelete(id: number): void {
    const ref = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '420px',
      data: {
        title: 'Brisanje rezervacije',
        message: 'Da li ste sigurni da želite obrisati rezervaciju?',
        confirmText: 'Obriši',
        cancelText: 'Odustani',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;

      this.isLoading = true;
      this.errorMessage = null;

      this.api
        .delete(id)
        .pipe(finalize(() => (this.isLoading = false)))
        .subscribe({
          next: () => this.load(),
          error: (err) => {
            console.error('Delete reservation error:', err);
            this.errorMessage =
              err?.error?.message ??
              err?.message ??
              'Greška prilikom brisanja rezervacije.';
          },
        });
    });
  }

  // Helpers (ako ih želiš koristiti u HTML-u)
  toShortTime(value?: string | null): string {
    if (!value) return '';
    return value.length >= 5 ? value.substring(0, 5) : value;
  }

  toShortDate(value?: string | null): string {
    if (!value) return '';
    return value.includes('T') ? value.split('T')[0] : value;
  }

  canModify(r: ListReservationQueryDto): boolean {
  const start = this.parseStartDateTime(r);
  if (!start) return false;

  const diffMs = start.getTime() - Date.now();
  const twoHoursMs = 2 * 60 * 60 * 1000;

  return diffMs >= twoHoursMs;
}

private parseStartDateTime(r: ListReservationQueryDto): Date | null {
  // Najčešće: ISO "2026-02-22T23:11:00"
  if (r.startTime && r.startTime.includes('T')) {
    const d = new Date(r.startTime);
    return isNaN(d.getTime()) ? null : d;
  }

  // Fallback: ako nekad dođe samo "HH:mm" + datum odvojeno
  const dateStr: any = (r as any).reservationDate ?? (r as any).date;
  if (!dateStr || !r.startTime) return null;

  const day = String(dateStr).includes('T') ? String(dateStr).split('T')[0] : String(dateStr);
  const iso = `${day}T${String(r.startTime).length === 5 ? r.startTime + ':00' : r.startTime}`;
  const d = new Date(iso);
  return isNaN(d.getTime()) ? null : d;
}

}
