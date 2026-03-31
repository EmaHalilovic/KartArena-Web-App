import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';

import {
  ListReservationRequest,
  ListReservationQueryDto,
} from '../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';

import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../core/services/toaster.service';
import { ConfirmDeleteDialogReservationComponent } from '../reservation/dialogs/confirm-delete/confirm-delete-dialog.component';

@Component({
  selector: 'app-reservation',
  standalone: false,
  templateUrl: './reservation.component.html',
  styleUrl: './reservation.component.scss',
})
export class ReservationComponent
  extends BaseListPagedComponent<ListReservationQueryDto, ListReservationRequest>
  implements OnInit
{
  private api = inject(ReservationApiService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);

displayedColumns: string[] = [
  'id',
  'user',
  'track',
  'kart',
  'startTime',
  'endTime',
  'status',
];

  // optional filter toggle (use only if your API supports it)
  showOnlyEnabled = false;

  constructor() {
    super();
    this.request = new ListReservationRequest();

    // ensure paging exists (depends on your models)
    this.request.paging = this.request.paging ?? { page: 1, pageSize: 10 };
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

    // keep request.search in sync if your html binds [(ngModel)]="request.search"
    this.request.search = this.request.search ?? null;

    this.api.list(this.request).subscribe({
      next: (response: any) => {
        // BaseListPagedComponent usually expects PageResult-like response
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load reservations error:', err);
        this.stopLoading('Failed to load reservations');
      },
    });
  }

  // === UI Actions ===

  onCreate(): void {
    this.router.navigate(['/client/reservation/add']);
  }

  onEdit(item: ListReservationQueryDto): void {
    this.router.navigate(['/admin/reservation/edit', item.id]);
  }

  onDelete(item: ListReservationQueryDto): void {
    const ref = this.dialog.open(ConfirmDeleteDialogReservationComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        title: 'Brisanje rezervacije',
        message: 'Da li ste sigurni da želite obrisati rezervaciju?',
        confirmText: 'Obriši',
        cancelText: 'Odustani',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.performDelete(item);
      }
    });
  }

  private performDelete(item: ListReservationQueryDto): void {
    this.startLoading();

    this.api.delete(item.id).subscribe({
      next: () => {
        this.toaster.success('Reservation deleted successfully');
        this.loadPagedData();
      },
      error: (err) => {
        console.error('Delete reservation error:', err);
        this.stopLoading('Failed to delete reservation');
      },
    });
  }

  onSearch(): void {
    // reset page
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // OPTIONAL: enable filter toggle like equipment (only if backend supports it)
  onToggleEnabledFilter(checked: boolean): void {
    this.showOnlyEnabled = checked;

    // If ListReservationRequest has onlyEnabled -> keep it.
    // If it doesn't exist in your model, REMOVE the next line.
    (this.request as any).onlyEnabled = checked;

    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // Helpers (optional for template)
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
    // ISO "2026-02-22T23:11:00"
    if (r.startTime && r.startTime.includes('T')) {
      const d = new Date(r.startTime);
      return isNaN(d.getTime()) ? null : d;
    }

    // Fallback: date + "HH:mm"
    const dateStr: any = (r as any).reservationDate ?? (r as any).date;
    if (!dateStr || !r.startTime) return null;

    const day = String(dateStr).includes('T') ? String(dateStr).split('T')[0] : String(dateStr);
    const time = String(r.startTime);
    const iso = `${day}T${time.length === 5 ? time + ':00' : time}`;

    const d = new Date(iso);
    return isNaN(d.getTime()) ? null : d;
  }

  getUserFullName(r: ListReservationQueryDto): string {
  const first = (r.userFirstName ?? '').trim();
  const last = (r.userLastName ?? '').trim();
  const full = `${first} ${last}`.trim();
  return full || `#${r.userId}`;
}

getTrackLabel(r: ListReservationQueryDto): string {
  return (r.trackName ?? '').trim() || `#${r.trackId}`;
}

getKartLabel(r: ListReservationQueryDto): string {
  return (r.kartName ?? '').trim() || `#${r.kartId}`;
}

getReservationLevel(r: ListReservationQueryDto): 'good' | 'fair' | 'low' {
  const start = this.parseStartDateTime(r);
  if (!start) return 'low';

  const diffMs = start.getTime() - Date.now();
  if (diffMs < 0) return 'low';
  if (diffMs < 2 * 60 * 60 * 1000) return 'fair';
  return 'good';
}

getReservationLabel(r: ListReservationQueryDto): string {
  const level = this.getReservationLevel(r);
  if (level === 'good') return 'Upcoming';
  if (level === 'fair') return 'Soon';
  return 'Past';
}

}
