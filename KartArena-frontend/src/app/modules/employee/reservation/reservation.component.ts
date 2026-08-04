import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { finalize, Observable } from 'rxjs';

import {
  ListReservationRequest,
  ListReservationQueryDto,
  MarkReservationCashPaidPayload,
  AssignReservationResourcesPayload,
  ReservationStatus,
} from '../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';
import { PaymentStatus } from '../../../api-services/payments/payments-api.models';

import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../core/services/toaster.service';
import { ConfirmDeleteDialogReservationComponent } from '../reservation/dialogs/confirm-delete/confirm-delete-dialog.component';
import { ReservationCashPaymentDialogComponent } from './dialogs/reservation-cash-payment-dialog/reservation-cash-payment-dialog.component';
import { ReservationAssignDialogComponent } from './dialogs/reservation-assign-dialog/reservation-assign-dialog.component';

type ReservationTone = 'good' | 'fair' | 'low';

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
  private readonly api = inject(ReservationApiService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly toaster = inject(ToasterService);

  displayedColumns: string[] = [
    'id',
    'user',
    'track',
    'date',
    'time',
    'status',
    'paymentStatus',
    'paymentAmount',
    'actions',
  ];

  readonly reservationStatusOptions = [
    { value: null, label: 'All reservation statuses' },
    { value: ReservationStatus.Pending, label: 'Pending' },
    { value: ReservationStatus.Confirmed, label: 'Confirmed' },
    { value: ReservationStatus.Completed, label: 'Completed' },
    { value: ReservationStatus.Cancelled, label: 'Cancelled' },
  ];

  readonly paymentStatusOptions = [
    { value: null, label: 'All payment statuses' },
    { value: PaymentStatus.Pending, label: 'Pending' },
    { value: PaymentStatus.Paid, label: 'Paid' },
    { value: PaymentStatus.Failed, label: 'Failed' },
    { value: PaymentStatus.Cancelled, label: 'Cancelled' },
    { value: PaymentStatus.Refunded, label: 'Refunded' },
  ];

  readonly ReservationStatus = ReservationStatus;

  onlyToday = false;
  onlyUpcoming = false;
  dateFromValue: Date | null = null;
  dateToValue: Date | null = null;

  constructor() {
    super();
    this.request = new ListReservationRequest();
    this.request.paging = this.request.paging ?? { page: 1, pageSize: 10 };
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.buildRequest()).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.applyClientSideUpcomingFilter();
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load reservations error:', err);
        this.stopLoading('Failed to load reservations');
      },
    });
  }

  onRetry(): void {
    this.loadPagedData();
  }



  onDelete(item: ListReservationQueryDto): void {
    const ref = this.dialog.open(ConfirmDeleteDialogReservationComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        titleKey: 'Delete reservation',
        messageKey: 'This will permanently remove the selected reservation.',
        confirmKey: 'Delete',
        cancelKey: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }

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
    });
  }

  onSearch(search: string): void {
    this.request.paging.page = 1;
    this.request.search = search?.trim() || null;
    this.loadPagedData();
  }

  onReservationStatusChange(value: ReservationStatus | null): void {
    this.request.status = value;
    this.refreshFromFirstPage();
  }

  onPaymentStatusChange(value: PaymentStatus | null): void {
    this.request.paymentStatus = value;
    this.refreshFromFirstPage();
  }

  onDateFromChange(value: Date | string | null): void {
    this.dateFromValue = this.normalizePickerDate(value);
    this.request.dateFrom = this.toIsoDate(value);
    this.refreshFromFirstPage();
  }

  onDateToChange(value: Date | string | null): void {
    this.dateToValue = this.normalizePickerDate(value);
    this.request.dateTo = this.toIsoDate(value);
    this.refreshFromFirstPage();
  }

  onOnlyTodayChange(checked: boolean): void {
    this.onlyToday = checked;
    if (checked) {
      this.onlyUpcoming = false;
    }

    this.refreshFromFirstPage();
  }

  onOnlyUpcomingChange(checked: boolean): void {
    this.onlyUpcoming = checked;
    if (checked) {
      this.onlyToday = false;
    }

    this.refreshFromFirstPage();
  }

  clearFilters(): void {
    this.request.status = null;
    this.request.paymentStatus = null;
    this.request.paymentTypeName = null;
    this.request.dateFrom = null;
    this.request.dateTo = null;
    this.dateFromValue = null;
    this.dateToValue = null;
    this.onlyToday = false;
    this.onlyUpcoming = false;
    this.refreshFromFirstPage();
  }

  openDetails(item: ListReservationQueryDto): void {
    this.router.navigate(['/employee/reservations', item.id]);
  }

  openEdit(item: ListReservationQueryDto): void {
    if (!this.canEdit(item)) {
      this.toaster.error('Only confirmed reservations can be edited');
      return;
    }

    this.router.navigate(['/employee/reservation/edit', item.id]);
  }

  openMarkCashPaid(item: ListReservationQueryDto): void {
    if (!this.canMarkCashPaid(item)) {
      this.toaster.error('Only confirmed reservations can be marked as paid in cash');
      return;
    }

    const ref = this.dialog.open(ReservationCashPaymentDialogComponent, {
      width: '480px',
      maxWidth: '96vw',
      data: {
        reservationCode: this.formatReservationCode(item),
        customerName: this.getUserFullName(item),
        amount: item.paymentAmount ?? null,
      },
    });

    ref.afterClosed().subscribe((payload?: MarkReservationCashPaidPayload) => {
      if (!payload) {
        return;
      }

      this.runRowAction(
        this.api.markCashPaid(item.id, payload),
        'Cash payment marked as paid',
        'Failed to confirm cash payment'
      );
    });
  }

  openAssignResources(item: ListReservationQueryDto): void {
    if (!this.canAssignResources(item)) {
      this.toaster.error('Only confirmed reservations can be assigned');
      return;
    }

    const ref = this.dialog.open(ReservationAssignDialogComponent, {
      width: '520px',
      maxWidth: '96vw',
      data: {
        reservationId: item.id,
        reservationCode: this.formatReservationCode(item),
        customerName: this.getUserFullName(item),
        assignedEmployeeName: item.assignedEmployeeName ?? null,
        assignedEquipmentItemId: item.assignedEquipmentItemId ?? null,
      },
    });

    ref.afterClosed().subscribe((payload?: AssignReservationResourcesPayload) => {
      if (!payload) {
        return;
      }

      this.runRowAction(
        this.api.assignResources(item.id, payload),
        'Reservation resources assigned successfully',
        'Failed to assign reservation resources'
      );
    });
  }

  changeReservationStatus(item: ListReservationQueryDto, status: ReservationStatus.Completed | ReservationStatus.Cancelled): void {
    if (!this.canCloseReservation(item)) {
      this.toaster.error('Confirmed reservations can only be closed on their reservation date');
      return;
    }

    const completed = status === ReservationStatus.Completed;
    const ref = this.dialog.open(ConfirmDeleteDialogReservationComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        titleKey: completed ? 'Complete reservation' : 'Cancel reservation',
        messageKey: completed
          ? 'Confirm that the customer completed this reservation.'
          : 'Confirm that the customer did not show up.',
        confirmKey: completed ? 'Complete' : 'Mark as no-show',
        cancelKey: 'Back',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.runRowAction(
        this.api.changeStatus(item.id, status),
        completed ? 'Reservation marked as completed' : 'Reservation cancelled as no-show',
        'Failed to update reservation status'
      );
    });
  }

  toShortTime(value?: string | null): string {
    if (!value) {
      return '';
    }

    const timeCandidate = value.includes('T') ? value.split('T')[1] ?? value : value;
    return timeCandidate.length >= 5 ? timeCandidate.substring(0, 5) : timeCandidate;
  }

  toShortDate(value?: string | null): string {
    if (!value) {
      return '';
    }

    return value.includes('T') ? value.split('T')[0] : value;
  }

  canDelete(r: ListReservationQueryDto): boolean {
    return this.isDeletableReservation(r);
  }

  canConfirmReservation(r: ListReservationQueryDto): boolean {
    return this.hasReservationStatus(r, ['pending', 'soon']) && !this.isClosedReservation(r);
  }

  canMarkCashPaid(r: ListReservationQueryDto): boolean {
    return this.isConfirmedReservation(r)
      && this.isCashPayment(r)
      && this.hasPaymentStatus(r, ['pending', 'awaitingpayment', 'processing']);
  }

  canEdit(r: ListReservationQueryDto): boolean {
    return this.isConfirmedReservation(r);
  }

  canAssignResources(r: ListReservationQueryDto): boolean {
    return this.isConfirmedReservation(r);
  }

  canCloseReservation(r: ListReservationQueryDto): boolean {
    return this.isConfirmedReservation(r) && this.toShortDate(r.date) === this.getTodayIsoDate();
  }

  getUserFullName(r: ListReservationQueryDto): string {
    const userName = this.joinName(
      this.readString(r, 'userFirstName', 'UserFirstName'),
      this.readString(r, 'userLastName', 'UserLastName')
    );
    const customerName = this.joinName(
      this.readString(r, 'customerFirstName', 'CustomerFirstName'),
      this.readString(r, 'customerLastName', 'CustomerLastName')
    );

    return userName || customerName || (r.userId ? `#${r.userId}` : '-');
  }

  private joinName(firstName: string, lastName: string): string {
    return `${firstName.trim()} ${lastName.trim()}`.trim();
  }

  private readString(source: unknown, ...keys: string[]): string {
    const record = source as Record<string, unknown>;
    const value = keys.map((key) => record[key]).find((item) => typeof item === 'string');
    return typeof value === 'string' ? value : '';
  }

  getTrackLabel(r: ListReservationQueryDto): string {
    return (r.trackName ?? '').trim() || `#${r.trackId}`;
  }

  getTimeRangeLabel(r: ListReservationQueryDto): string {
    const start = this.toShortTime(r.startTime);
    const end = this.toShortTime(r.endTime);

    if (start && end) {
      return `${start} - ${end}`;
    }

    return start || end || '-';
  }

  getReservationStatusLabel(r: ListReservationQueryDto): string {
    return this.normalizeBackendStatus(r.status)?.label ?? this.getFallbackStatus(r).label;
  }

  getReservationStatusClass(r: ListReservationQueryDto): ReservationTone {
    return this.normalizeBackendStatus(r.status)?.className ?? this.getFallbackStatus(r).className;
  }

  getPaymentAmountLabel(r: ListReservationQueryDto): string {
    if (r.paymentAmount === null || r.paymentAmount === undefined) {
      return '-';
    }

    return r.paymentAmount.toFixed(2);
  }

  getPaymentStatusLabel(r: ListReservationQueryDto): string {
    return this.normalizePaymentStatus(r.paymentStatus)?.label ?? '-';
  }

  getPaymentStatusClass(r: ListReservationQueryDto): ReservationTone {
    return this.normalizePaymentStatus(r.paymentStatus)?.className ?? 'fair';
  }

  getRowStateClass(r: ListReservationQueryDto): string | null {
    if (this.hasReservationStatus(r, ['cancelled', 'canceled', 'expired', 'past'])) {
      return 'row-problem';
    }

    if (this.hasReservationStatus(r, ['completed'])) {
      return 'row-complete';
    }

    if (this.canMarkCashPaid(r)) {
      return 'row-payment-attention';
    }

    if (this.isStartingSoon(r)) {
      return 'row-urgent';
    }

    return null;
  }

  hasActiveFilters(): boolean {
    return Boolean(
      this.request.search ||
      this.request.status != null ||
      this.request.paymentStatus != null ||
      this.request.paymentTypeName ||
      this.request.dateFrom ||
      this.request.dateTo ||
      this.onlyToday ||
      this.onlyUpcoming
    );
  }

  get visibleReservationsCount(): number {
    return this.items.length;
  }

  get pendingPaymentCount(): number {
    return this.items.filter((item) => this.isPaymentAttentionReservation(item)).length;
  }

  get startingSoonCount(): number {
    return this.items.filter((item) => this.isStartingSoonReservation(item)).length;
  }

  get confirmedCount(): number {
    return this.items.filter((item) => this.isConfirmedReservation(item)).length;
  }

  private refreshFromFirstPage(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  private buildRequest(): ListReservationRequest {
    const request = new ListReservationRequest();
    request.paging = this.request.paging;
    request.search = this.request.search?.trim() || null;
    request.userId = this.request.userId ?? null;
    request.trackId = this.request.trackId ?? null;
    request.kartId = this.request.kartId ?? null;
    request.status = this.request.status ?? null;
    request.paymentStatus = this.request.paymentStatus ?? null;
    request.paymentTypeName = this.request.paymentTypeName ?? null;

    if (this.onlyToday) {
      const today = this.getTodayIsoDate();
      request.dateFrom = today;
      request.dateTo = today;
      return request;
    }

    request.dateFrom = this.onlyUpcoming ? this.request.dateFrom ?? this.getTodayIsoDate() : this.request.dateFrom ?? null;
    request.dateTo = this.request.dateTo ?? null;
    return request;
  }

  private applyClientSideUpcomingFilter(): void {
    if (!this.onlyUpcoming) {
      return;
    }

    const now = Date.now();
    this.items = this.items.filter((item) => {
      const start = this.parseStartDateTime(item);
      return !start || start.getTime() >= now;
    });
  }

  private runRowAction(operation$: Observable<unknown>, successMessage: string, errorMessage: string): void {
    this.startLoading();
    operation$
      .pipe(finalize(() => this.stopLoading()))
      .subscribe({
        next: () => {
          this.toaster.success(successMessage);
          this.loadPagedData();
        },
        error: (err) => {
          console.error(errorMessage, err);
          this.toaster.error(err?.message || errorMessage);
        },
      });
  }

  private formatReservationCode(r: ListReservationQueryDto): string {
    return `RSV-${String(r.id).padStart(3, '0')}`;
  }

  private getFallbackStatus(r: ListReservationQueryDto): { label: string; className: ReservationTone } {
    const start = this.parseStartDateTime(r);
    if (!start) {
      return { label: 'Unknown', className: 'low' };
    }

    const diffMs = start.getTime() - Date.now();
    if (diffMs < 0) {
      return { label: 'Past', className: 'low' };
    }

    if (diffMs < 2 * 60 * 60 * 1000) {
      return { label: 'Soon', className: 'fair' };
    }

    return { label: 'Upcoming', className: 'good' };
  }

  private normalizeBackendStatus(
    status: ReservationStatus | string | number | null | undefined
  ): { label: string; className: ReservationTone } | null {
    if (status === null || status === undefined || status === '') {
      return null;
    }

    if (typeof status === 'number') {
      switch (status) {
        case ReservationStatus.Pending:
          return { label: 'Pending', className: 'fair' };
        case ReservationStatus.Confirmed:
          return { label: 'Confirmed', className: 'good' };
        case ReservationStatus.Completed:
          return { label: 'Completed', className: 'good' };
        case ReservationStatus.Cancelled:
          return { label: 'Cancelled', className: 'low' };
        default:
          return { label: String(status), className: 'fair' };
      }
    }

    const normalized = status.trim();
    if (!normalized) {
      return null;
    }

    const key = this.normalizeKey(normalized);
    if (['confirmed', 'active', 'approved', 'booked', 'scheduled', 'completed', 'finished'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'good' };
    }

    if (['pending', 'soon', 'awaitingpayment', 'inprogress'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'fair' };
    }

    if (['cancelled', 'canceled', 'failed', 'rejected', 'expired', 'past', 'noshow'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'low' };
    }

    return { label: this.humanizeStatus(normalized), className: 'fair' };
  }

  private normalizePaymentStatus(
    status: PaymentStatus | string | number | null | undefined
  ): { label: string; className: ReservationTone } | null {
    if (status === null || status === undefined || status === '') {
      return null;
    }

    if (typeof status === 'number') {
      switch (status) {
        case PaymentStatus.Pending:
          return { label: 'Pending', className: 'fair' };
        case PaymentStatus.Paid:
          return { label: 'Paid', className: 'good' };
        case PaymentStatus.Failed:
          return { label: 'Failed', className: 'low' };
        case PaymentStatus.Cancelled:
          return { label: 'Cancelled', className: 'low' };
        case PaymentStatus.Refunded:
          return { label: 'Refunded', className: 'fair' };
        default:
          return { label: String(status), className: 'fair' };
      }
    }

    const normalized = status.trim();
    if (!normalized) {
      return null;
    }

    const key = this.normalizeKey(normalized);
    if (['paid', 'completed', 'success', 'successful'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'good' };
    }

    if (['pending', 'processing', 'awaitingpayment', 'refunded'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'fair' };
    }

    if (['failed', 'cancelled', 'canceled', 'refused'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'low' };
    }

    return { label: this.humanizeStatus(normalized), className: 'fair' };
  }

  private isConfirmedReservation(r: ListReservationQueryDto): boolean {
    return this.hasReservationStatus(r, ['confirmed', 'active', 'approved', 'booked', 'scheduled']);
  }

  private isPaymentAttentionReservation(r: ListReservationQueryDto): boolean {
    return this.hasPaymentStatus(r, ['pending', 'awaitingpayment', 'processing']);
  }

  private isStartingSoonReservation(r: ListReservationQueryDto): boolean {
    if (this.isClosedReservation(r)) {
      return false;
    }

    return this.isStartingSoon(r);
  }

  private humanizeStatus(value: string): string {
    return value
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim()
      .replace(/\b\w/g, (char) => char.toUpperCase());
  }

  private parseStartDateTime(r: ListReservationQueryDto): Date | null {
    const parsedStart = this.tryParseDate(r.startTime);
    if (parsedStart) {
      return parsedStart;
    }

    const day = this.extractDatePart(r.date);
    const time = this.normalizeTime(r.startTime);
    if (!day || !time) {
      return null;
    }

    return this.tryParseDate(`${day}T${time}`);
  }

  private normalizeTime(value?: string | null): string | null {
    if (!value) {
      return null;
    }

    const trimmed = value.trim();
    if (!trimmed) {
      return null;
    }

    if (trimmed.includes('T')) {
      const timePart = trimmed.split('T')[1]?.split('.')[0] ?? '';
      return this.normalizeTime(timePart);
    }

    const match = trimmed.match(/^(\d{2}):(\d{2})(?::(\d{2}))?$/);
    if (!match) {
      return null;
    }

    const [, hh, mm, ss] = match;
    return `${hh}:${mm}:${ss ?? '00'}`;
  }

  private extractDatePart(value?: string | null): string | null {
    if (!value) {
      return null;
    }

    const trimmed = value.trim();
    if (!trimmed) {
      return null;
    }

    const datePart = trimmed.includes('T') ? trimmed.split('T')[0] : trimmed;
    return /^\d{4}-\d{2}-\d{2}$/.test(datePart) ? datePart : null;
  }

  private tryParseDate(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  private toIsoDate(value: Date | string | null): string | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      const year = value.getFullYear();
      const month = String(value.getMonth() + 1).padStart(2, '0');
      const day = String(value.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    }

    return value.includes('T') ? value.split('T')[0] : value;
  }

  private normalizePickerDate(value: Date | string | null): Date | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? null : value;
    }

    return this.tryParseDate(value);
  }

  private getTodayIsoDate(): string {
    return this.toIsoDate(new Date()) ?? '';
  }

  private normalizeKey(value: string | number | null | undefined): string {
    return String(value ?? '')
      .trim()
      .toLowerCase()
      .replace(/[\s_-]+/g, '');
  }

  private getReservationStatusKey(status: ReservationStatus | string | number | null | undefined): string {
    const normalized = this.normalizeBackendStatus(status);
    return this.normalizeKey(normalized?.label ?? status);
  }

  private getPaymentStatusKey(status: PaymentStatus | string | number | null | undefined): string {
    const normalized = this.normalizePaymentStatus(status);
    return this.normalizeKey(normalized?.label ?? status);
  }

  private hasReservationStatus(r: ListReservationQueryDto, matches: string[]): boolean {
    return matches.includes(this.getReservationStatusKey(r.status));
  }

  private hasPaymentStatus(r: ListReservationQueryDto, matches: string[]): boolean {
    return matches.includes(this.getPaymentStatusKey(r.paymentStatus));
  }

  private isCashPayment(r: ListReservationQueryDto): boolean {
    return this.normalizeKey(r.paymentTypeName).includes('cash');
  }

  private isDeletableReservation(r: ListReservationQueryDto): boolean {
    return this.hasReservationStatus(r, ['completed', 'cancelled', 'canceled']);
  }

  private isClosedReservation(r: ListReservationQueryDto): boolean {
    return this.hasReservationStatus(r, ['cancelled', 'canceled', 'completed', 'expired', 'past']);
  }

  private isStartingSoon(r: ListReservationQueryDto): boolean {
    const start = this.parseStartDateTime(r);
    if (!start) {
      return false;
    }

    const diffMs = start.getTime() - Date.now();
    return diffMs >= 0 && diffMs <= 90 * 60 * 1000;
  }
}
