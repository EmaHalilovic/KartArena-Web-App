import { Component, inject, OnInit } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { EquipmentStockStatus, ListEquipmentRequest, ListEquipmentResponse } from '../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../api-services/equipment/equipment-api.service';
import { ListKartsRequest, ListKartsQueryDto } from '../../../api-services/karts/karts-api.models';
import { KartsApiService } from '../../../api-services/karts/karts-api.service';
import { ListPaymentsQueryDto, ListPaymentsRequest, PaymentStatus } from '../../../api-services/payments/payments-api.models';
import { PaymentsApiService } from '../../../api-services/payments/payments-api.service';
import { ListReservationQueryDto, ListReservationRequest, ReservationStatus } from '../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';
import { PageResult } from '../../../core/models/paging/page-result';
import { addDaysToDateKey, getBusinessDateKey } from '../../../core/services/business-date-time';

interface DashboardMetric {
  label: string;
  value: string;
  detail: string;
  icon: string;
  tone: 'red' | 'blue' | 'green' | 'amber';
}

interface DailyReservationCount {
  label: string;
  count: number;
  height: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly reservationsApi = inject(ReservationApiService);
  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly kartsApi = inject(KartsApiService);
  private readonly equipmentApi = inject(EquipmentApiService);

  loading = true;
  metrics: DashboardMetric[] = [];
  recentReservations: ListReservationQueryDto[] = [];
  reservationChart: DailyReservationCount[] = [];
  attentionItems: { label: string; value: string; icon: string }[] = [];

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    const reservationsRequest = new ListReservationRequest();
    reservationsRequest.paging.page = 1;
    reservationsRequest.paging.pageSize = 250;

    const paymentsRequest = new ListPaymentsRequest();
    paymentsRequest.paging.page = 1;
    paymentsRequest.paging.pageSize = 250;

    const kartsRequest = new ListKartsRequest();
    kartsRequest.paging.page = 1;
    kartsRequest.paging.pageSize = 250;
    kartsRequest.onlyEnabled = true;

    const equipmentRequest = new ListEquipmentRequest();
    equipmentRequest.paging.page = 1;
    equipmentRequest.paging.pageSize = 250;
    equipmentRequest.onlyActive = true;

    forkJoin({
      reservations: this.reservationsApi.list(reservationsRequest).pipe(
        catchError(() => of(this.emptyPage<ListReservationQueryDto>()))
      ),
      payments: this.paymentsApi.list(paymentsRequest).pipe(
        catchError(() => of(this.emptyPage<ListPaymentsQueryDto>()))
      ),
      karts: this.kartsApi.list(kartsRequest).pipe(
        catchError(() => of(this.emptyPage<ListKartsQueryDto>()))
      ),
      equipment: this.equipmentApi.list(equipmentRequest).pipe(
        catchError(() => of(this.emptyPage<ListEquipmentResponse['items'][number]>()))
      ),
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(({ reservations, payments, karts, equipment }) => {
        this.populateDashboard(reservations, payments, karts, equipment);
      });
  }

  getReservationStatusLabel(status?: ReservationStatus | null): string {
    return ReservationStatus[status ?? ReservationStatus.Pending] ?? 'Pending';
  }

  private populateDashboard(
    reservations: PageResult<ListReservationQueryDto>,
    payments: PageResult<ListPaymentsQueryDto>,
    karts: PageResult<ListKartsQueryDto>,
    equipment: ListEquipmentResponse
  ): void {
    const today = getBusinessDateKey();
    const month = today.slice(0, 7);
    const reservationItems = reservations.items ?? [];
    const paymentItems = payments.items ?? [];
    const equipmentItems = equipment.items ?? [];
    const todayReservations = reservationItems.filter((item) => this.toDateKey(item.date) === today).length;
    const todayRevenue = paymentItems
      .filter((item) => item.status === PaymentStatus.Paid && this.toDateKey(item.paymentDate) === today)
      .reduce((sum, item) => sum + (item.amount ?? 0), 0);
    const monthRevenue = paymentItems
      .filter((item) => item.status === PaymentStatus.Paid && this.toDateKey(item.paymentDate).startsWith(month))
      .reduce((sum, item) => sum + (item.amount ?? 0), 0);
    const attentionEquipment = equipmentItems.filter((item) => item.stockStatus !== EquipmentStockStatus.Good);

    this.metrics = [
      { label: 'Today\'s reservations', value: String(todayReservations), detail: 'Scheduled for today', icon: 'calendar_month', tone: 'red' },
      { label: 'Revenue today', value: this.money(todayRevenue), detail: 'Paid transactions', icon: 'payments', tone: 'green' },
      { label: 'Revenue this month', value: this.money(monthRevenue), detail: 'Paid transactions', icon: 'trending_up', tone: 'blue' },
      { label: 'Available karts', value: String(karts.totalItems ?? karts.items.length), detail: 'Enabled in the fleet', icon: 'directions_car', tone: 'amber' },
    ];

    this.recentReservations = [...reservationItems]
      .sort((left, right) => this.reservationDateTime(right).localeCompare(this.reservationDateTime(left)))
      .slice(0, 5);
    this.reservationChart = this.buildReservationChart(reservationItems);
    this.attentionItems = [
      ...attentionEquipment.slice(0, 3).map((item) => ({
        label: item.name,
        value: item.stockStatus === EquipmentStockStatus.Low ? 'Low stock' : 'Out of stock',
        icon: 'construction',
      })),
      ...(reservations.totalItems ?? reservationItems.length) > 0
        ? [{ label: 'Reservations', value: `${reservations.totalItems ?? reservationItems.length} total records`, icon: 'event_note' }]
        : [],
    ];
  }

  private buildReservationChart(items: ListReservationQueryDto[]): DailyReservationCount[] {
    const today = getBusinessDateKey();
    const days = Array.from({ length: 7 }, (_, index) => addDaysToDateKey(today, index - 6));
    const counts = days.map((day) => items.filter((item) => this.toDateKey(item.date) === day).length);
    const highest = Math.max(...counts, 1);

    return days.map((day, index) => ({
      label: new Date(`${day}T12:00:00Z`).toLocaleDateString('en', { weekday: 'short', timeZone: 'UTC' }),
      count: counts[index],
      height: Math.max(10, (counts[index] / highest) * 100),
    }));
  }

  private emptyPage<T>(): PageResult<T> {
    return { items: [], totalItems: 0 };
  }

  private reservationDateTime(item: ListReservationQueryDto): string {
    return `${this.toDateKey(item.date)}T${item.startTime ?? ''}`;
  }

  private toDateKey(value?: string | Date | null): string {
    if (!value) return '';
    if (value instanceof Date) return value.toISOString().slice(0, 10);
    return value.includes('T') ? value.slice(0, 10) : value;
  }

  private money(value: number): string {
    return new Intl.NumberFormat('en-BA', { style: 'currency', currency: 'BAM', maximumFractionDigits: 0 }).format(value);
  }
}
