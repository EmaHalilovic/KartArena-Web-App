import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import {
  getBusinessDateTimeKey,
} from '../../../core/services/business-date-time';
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

  data: ListReservationQueryDto[] = [];

  displayedColumns: string[] = ['id', 'userId', 'trackId', 'kartId', 'date', 'startTime', 'endTime', 'actions'];

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
          this.data = res.items ?? [];
          this.totalCount = res.total ?? res.totalItems ?? this.data.length;
        },
        error: (err: unknown) => {
          console.error('Load reservations error:', err);
          this.errorMessage = this.readErrorMessage(
            err,
            'Greška prilikom učitavanja rezervacija.'
          );
        },
      });
  }

  onAdd(): void {
    this.router.navigate(['/client/reservation/add']);
  }

  onEdit(id: number): void {
    this.router.navigate(['/client/reservation/edit', id]);
  }

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
          error: (err: unknown) => {
            console.error('Delete reservation error:', err);
            this.errorMessage = this.readErrorMessage(
              err,
              'Greška prilikom brisanja rezervacije.'
            );
          },
        });
    });
  }

  toShortTime(value?: string | null): string {
    if (!value) return '';
    return value.length >= 5 ? value.substring(0, 5) : value;
  }

  toShortDate(value?: string | null): string {
    if (!value) return '';
    return value.includes('T') ? value.split('T')[0] : value;
  }

  canModify(r: ListReservationQueryDto): boolean {
  const startKey = this.getReservationStartKey(r);
  if (!startKey) return false;

  const nowKey = getBusinessDateTimeKey();

  const startMs =
    this.dateTimeKeyToComparableValue(startKey);

  const nowMs =
    this.dateTimeKeyToComparableValue(nowKey);

  const twoHoursMs =
    2 * 60 * 60 * 1000;

  return startMs - nowMs >= twoHoursMs;
}

private getReservationStartKey(
  r: ListReservationQueryDto
): string | null {
  if (!r.date || !r.startTime) {
    return null;
  }

  const day = r.date.includes('T')
    ? r.date.split('T')[0]
    : r.date;

  let time = r.startTime.includes('T')
    ? r.startTime.split('T')[1]
    : r.startTime;

  if (time.length === 5) {
    time += ':00';
  }

  return `${day}T${time.substring(0, 8)}`;
}

private dateTimeKeyToComparableValue(
  value: string
): number {
  const [datePart, timePart] =
    value.split('T');

  const [year, month, day] =
    datePart.split('-').map(Number);

  const [hour, minute, second = 0] =
    timePart.split(':').map(Number);

  return Date.UTC(
    year,
    month - 1,
    day,
    hour,
    minute,
    second
  );
}

 

  private readErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (this.hasMessage(error.error)) return error.error.message;
      if (error.message) return error.message;
    }

    return error instanceof Error && error.message ? error.message : fallback;
  }

  private hasMessage(value: unknown): value is { message: string } {
    return (
      typeof value === 'object' &&
      value !== null &&
      'message' in value &&
      typeof value.message === 'string'
    );
  }

}
