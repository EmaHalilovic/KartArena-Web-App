import { Component, inject, OnInit } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { EquipmentStockStatus, ListEquipmentQueryDto, ListEquipmentRequest } from '../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../api-services/equipment/equipment-api.service';
import { ListReservationQueryDto, ListReservationRequest, ReservationStatus } from '../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';
import { PageResult } from '../../../core/models/paging/page-result';
import { getBusinessDateKey, getBusinessDateTimeKey } from '../../../core/services/business-date-time';

@Component({
  selector: 'app-employee-dashboard',
  standalone: false,
  templateUrl: './employee-dashboard.component.html',
  styleUrl: './employee-dashboard.component.scss',
})
export class EmployeeDashboardComponent implements OnInit {
  private readonly reservationsApi = inject(ReservationApiService);
  private readonly equipmentApi = inject(EquipmentApiService);

  loading = true;
  todayReservations: ListReservationQueryDto[] = [];
  upcomingReservations: ListReservationQueryDto[] = [];
  lowStockEquipment: ListEquipmentQueryDto[] = [];
  todayCount = 0;
  upcomingCount = 0;
  readonly showUpcoming = localStorage.getItem('kart-arena-employee-show-upcoming') !== 'false';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    const reservationsRequest = new ListReservationRequest();
    reservationsRequest.paging.page = 1;
    reservationsRequest.paging.pageSize = 250;

    const equipmentRequest = new ListEquipmentRequest();
    equipmentRequest.paging.page = 1;
    equipmentRequest.paging.pageSize = 250;
    equipmentRequest.onlyActive = true;

    forkJoin({
      reservations: this.reservationsApi.list(reservationsRequest).pipe(
        catchError(() => of(this.emptyPage<ListReservationQueryDto>()))
      ),
      equipment: this.equipmentApi.list(equipmentRequest).pipe(
        catchError(() => of(this.emptyPage<ListEquipmentQueryDto>()))
      ),
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(({ reservations, equipment }) => this.populateDashboard(reservations, equipment));
  }

  statusLabel(status?: ReservationStatus | null): string {
    return ReservationStatus[status ?? ReservationStatus.Pending] ?? 'Pending';
  }

  private populateDashboard(
    reservations: PageResult<ListReservationQueryDto>,
    equipment: PageResult<ListEquipmentQueryDto>
  ): void {
    const today = getBusinessDateKey();
    const now = getBusinessDateTimeKey();
    const reservationItems = reservations.items ?? [];

    this.todayReservations = reservationItems
      .filter((item) => this.dateKey(item.date) === today)
      .sort((left, right) => left.startTime.localeCompare(right.startTime));
    this.upcomingReservations = reservationItems
      .filter((item) => this.reservationDateTimeKey(item) >= now && this.dateKey(item.date) !== today)
      .sort((left, right) => this.reservationDateTimeKey(left).localeCompare(this.reservationDateTimeKey(right)))
      .slice(0, 5);
    this.todayCount = this.todayReservations.length;
    this.upcomingCount = this.upcomingReservations.length;
    this.lowStockEquipment = (equipment.items ?? [])
      .filter((item) => item.stockStatus !== EquipmentStockStatus.Good)
      .slice(0, 5);
  }

  private reservationDateTimeKey(item: ListReservationQueryDto): string {
    return `${this.dateKey(item.date)}T${item.startTime}`;
  }

  private dateKey(value: string | Date): string {
    if (value instanceof Date) {
      const year = value.getFullYear();
      const month = String(value.getMonth() + 1).padStart(2, '0');
      const day = String(value.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    }
    return value.includes('T') ? value.slice(0, 10) : value;
  }

  private emptyPage<T>(): PageResult<T> {
    return { items: [], totalItems: 0 };
  }
}
