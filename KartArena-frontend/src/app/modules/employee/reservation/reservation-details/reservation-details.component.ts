import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Observable, finalize } from 'rxjs';

import {
  AssignReservationResourcesPayload,
  GetReservationByIdQueryDto,
  MarkReservationCashPaidPayload,
  ReservationEmployeeAssignmentDto,
  ReservationStatus,
} from '../../../../api-services/reservations/reservation-api.models';
import { PaymentStatus } from '../../../../api-services/payments/payments-api.models';
import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { ConfirmDeleteDialogReservationComponent } from '../dialogs/confirm-delete/confirm-delete-dialog.component';
import { ReservationCashPaymentDialogComponent } from '../dialogs/reservation-cash-payment-dialog/reservation-cash-payment-dialog.component';
import { ReservationAssignDialogComponent } from '../dialogs/reservation-assign-dialog/reservation-assign-dialog.component';

type ReservationTone = 'good' | 'fair' | 'low' | 'neutral';

@Component({
  selector: 'app-reservation-details',
  standalone: false,
  templateUrl: './reservation-details.component.html',
  styleUrl: './reservation-details.component.scss',
})
export class ReservationDetailsComponent implements OnInit {
  readonly ReservationStatus = ReservationStatus;
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly api = inject(ReservationApiService);
  private readonly toaster = inject(ToasterService);

  reservation: GetReservationByIdQueryDto | null = null;
  reservationId: number | null = null;
  assignments: ReservationEmployeeAssignmentDto[] = [];
  isLoading = false;
  isAssignmentsLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!Number.isFinite(id) || id <= 0) {
        this.reservationId = null;
        this.reservation = null;
        this.errorMessage = 'Reservation not found.';
        return;
      }

      this.reservationId = id;
      this.loadReservation();
    });
  }

  loadReservation(): void {
    if (!this.reservationId) {
      this.errorMessage = 'Reservation not found.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.api.getById(this.reservationId).subscribe({
      next: (reservation) => {
        this.reservation = reservation;
        this.isLoading = false;
        this.loadAssignments();
      },
      error: (err) => {
        console.error('Load reservation details error:', err);
        this.reservation = null;
        this.assignments = [];
        this.errorMessage = err?.status === 404
          ? 'Reservation not found or no longer available.'
          : 'Failed to load reservation details.';
        this.isLoading = false;
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/employee/reservations']);
  }

  openMarkCashPaid(): void {
    if (!this.reservation) {
      return;
    }

    if (!this.canMarkCashPaid()) {
      this.toaster.error('Only confirmed reservations can be marked as paid in cash');
      return;
    }

    const ref = this.dialog.open(ReservationCashPaymentDialogComponent, {
      width: '480px',
      maxWidth: '96vw',
      data: {
        reservationCode: this.reservationCode,
        customerName: this.userLabel,
        amount: this.reservation.paymentAmount ?? null,
      },
    });

    ref.afterClosed().subscribe((payload?: MarkReservationCashPaidPayload) => {
      if (!payload || !this.reservation) {
        return;
      }

      this.runRowAction(
        this.api.markCashPaid(this.reservation.id, payload),
        'Cash payment marked as paid',
        'Failed to confirm cash payment'
      );
    });
  }

  openAssignResources(): void {
    if (!this.reservation) {
      return;
    }

    if (!this.canAssignResources()) {
      this.toaster.error('Only confirmed reservations can be assigned');
      return;
    }

    const ref = this.dialog.open(ReservationAssignDialogComponent, {
      width: '520px',
      maxWidth: '96vw',
      data: {
        reservationId: this.reservation.id,
        reservationCode: this.reservationCode,
        customerName: this.userLabel,
        assignedEmployeeName: this.reservation.assignedEmployeeName ?? null,
        assignedEquipmentItemId: this.reservation.assignedEquipmentItemId ?? null,
      },
    });

    ref.afterClosed().subscribe((payload?: AssignReservationResourcesPayload) => {
      if (!payload || !this.reservation) {
        return;
      }

      this.runRowAction(
        this.api.assignResources(this.reservation.id, payload),
        'Reservation resources assigned successfully',
        'Failed to assign reservation resources'
      );
    });
  }

  onDelete(): void {
    if (!this.reservation) {
      return;
    }

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
      if (!confirmed || !this.reservation) {
        return;
      }

      this.isLoading = true;
      this.api.delete(this.reservation.id)
        .pipe(finalize(() => { this.isLoading = false; }))
        .subscribe({
          next: () => {
            this.toaster.success('Reservation deleted successfully');
            this.router.navigate(['/employee/reservations']);
          },
          error: (err) => {
            console.error('Delete reservation error:', err);
            this.toaster.error('Failed to delete reservation');
          },
        });
    });
  }

  get reservationCode(): string {
    return this.reservation ? `RSV-${String(this.reservation.id).padStart(3, '0')}` : 'Reservation';
  }

  get userLabel(): string {
    const reservation = this.reservation;
    if (!reservation) {
      return '-';
    }

    const userName = this.joinName(
      this.readString(reservation, 'userFirstName', 'UserFirstName'),
      this.readString(reservation, 'userLastName', 'UserLastName')
    );
    const customerName = this.joinName(
      this.readString(reservation, 'customerFirstName', 'CustomerFirstName'),
      this.readString(reservation, 'customerLastName', 'CustomerLastName')
    );

    return userName || customerName || (reservation.userId ? `#${reservation.userId}` : '-');
  }

  changeReservationStatus(status: ReservationStatus.Completed | ReservationStatus.Cancelled): void {
    if (!this.reservation || !this.canCloseReservation()) {
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
      if (!confirmed || !this.reservation) return;
      this.runRowAction(
        this.api.changeStatus(this.reservation.id, status),
        completed ? 'Reservation marked as completed' : 'Reservation cancelled as no-show',
        'Failed to update reservation status'
      );
    });
  }

  private joinName(firstName: string, lastName: string): string {
    return `${firstName.trim()} ${lastName.trim()}`.trim();
  }

  private readString(source: unknown, ...keys: string[]): string {
    const record = source as Record<string, unknown>;
    const value = keys.map((key) => record[key]).find((item) => typeof item === 'string');
    return typeof value === 'string' ? value : '';
  }

  get trackLabel(): string {
    const reservation = this.reservation;
    return reservation ? (reservation.trackName ?? '').trim() || `#${reservation.trackId}` : '-';
  }

  get kartLabel(): string {
    const reservation = this.reservation;
    return reservation ? (reservation.kartName ?? '').trim() || `#${reservation.kartId}` : '-';
  }

  get paymentTypeLabel(): string {
    return (this.reservation?.paymentTypeName ?? '').trim() || '-';
  }

  get paymentAmountLabel(): string {
    const amount = this.reservation?.paymentAmount;
    return amount == null ? '-' : amount.toFixed(2);
  }

  get assignedEmployeeLabel(): string {
    const uniqueNames = [...new Set(this.assignments.map((item) => item.employeeName?.trim()).filter(Boolean))];
    if (uniqueNames.length === 1) {
      return uniqueNames[0];
    }

    if (uniqueNames.length > 1) {
      return uniqueNames.join(', ');
    }

    return 'Unassigned';
  }

  get assignmentEquipmentLabels(): string[] {
    return this.assignments
      .map((item) => item.equipmentItemName?.trim())
      .filter((item): item is string => !!item);
  }

  get hasAssignments(): boolean {
    return this.assignmentEquipmentLabels.length > 0;
  }

  get reservationStatusLabel(): string {
    return this.normalizeReservationStatus(this.reservation?.status).label;
  }

  get reservationStatusTone(): ReservationTone {
    return this.normalizeReservationStatus(this.reservation?.status).className;
  }

  get paymentStatusLabel(): string {
    return this.normalizePaymentStatus(this.reservation?.paymentStatus).label;
  }

  get paymentStatusTone(): ReservationTone {
    return this.normalizePaymentStatus(this.reservation?.paymentStatus).className;
  }

  get dateLabel(): string {
    return this.toShortDate(this.reservation?.date);
  }

  get timeLabel(): string {
    if (!this.reservation) {
      return '-';
    }

    return `${this.toShortTime(this.reservation.startTime)} - ${this.toShortTime(this.reservation.endTime)}`;
  }

  get hasMetadata(): boolean {
    return Boolean(this.reservation?.createdAt || this.reservation?.modifiedAt);
  }

  get createdAtLabel(): string {
    return this.toDateTimeLabel(this.reservation?.createdAt);
  }

  get modifiedAtLabel(): string {
    return this.toDateTimeLabel(this.reservation?.modifiedAt);
  }

  canMarkCashPaid(): boolean {
    const reservation = this.reservation;
    return !!reservation
      && this.isConfirmedReservation(reservation)
      && this.isCashPayment(reservation)
      && this.hasPaymentStatus(reservation, ['pending', 'awaitingpayment', 'processing']);
  }

  canAssignResources(): boolean {
    const reservation = this.reservation;
    return !!reservation && this.isConfirmedReservation(reservation);
  }

  canCloseReservation(): boolean {
    const reservation = this.reservation;
    return !!reservation
      && this.isConfirmedReservation(reservation)
      && this.toShortDate(reservation.date) === this.toLocalIsoDate(new Date());
  }

  private toLocalIsoDate(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  canDelete(): boolean {
    return !!this.reservation && this.isDeletableReservation(this.reservation);
  }

  toShortDate(value?: string | null): string {
    if (!value) {
      return '-';
    }

    return value.includes('T') ? value.split('T')[0] : value;
  }

  toShortTime(value?: string | null): string {
    if (!value) {
      return '-';
    }

    const timeCandidate = value.includes('T') ? value.split('T')[1] ?? value : value;
    return timeCandidate.length >= 5 ? timeCandidate.substring(0, 5) : timeCandidate;
  }

  toDateTimeLabel(value?: string | null): string {
    if (!value) {
      return '-';
    }

    const parsed = this.tryParseDate(value);
    return parsed ? parsed.toLocaleString() : value;
  }

  private runRowAction(operation$: Observable<unknown>, successMessage: string, errorMessage: string): void {
    this.isLoading = true;
    operation$
      .pipe(finalize(() => { this.isLoading = false; }))
      .subscribe({
        next: () => {
          this.toaster.success(successMessage);
          this.loadReservation();
        },
        error: (err) => {
          console.error(errorMessage, err);
          this.toaster.error(err?.message || errorMessage);
        },
      });
  }

  private loadAssignments(): void {
    if (!this.reservationId) {
      this.assignments = [];
      return;
    }

    this.isAssignmentsLoading = true;
    this.api.getReservationAssignments(this.reservationId).subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.isAssignmentsLoading = false;
      },
      error: (err) => {
        console.error('Load reservation assignments error:', err);
        this.assignments = [];
        this.isAssignmentsLoading = false;
      },
    });
  }

  private normalizeReservationStatus(
    status: ReservationStatus | string | number | null | undefined
  ): { label: string; className: ReservationTone } {
    if (status === null || status === undefined || status === '') {
      return { label: '-', className: 'neutral' };
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
          return { label: String(status), className: 'neutral' };
      }
    }

    const normalized = status.trim();
    if (!normalized) {
      return { label: '-', className: 'neutral' };
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

    return { label: this.humanizeStatus(normalized), className: 'neutral' };
  }

  private normalizePaymentStatus(
    status: PaymentStatus | string | number | null | undefined
  ): { label: string; className: ReservationTone } {
    if (status === null || status === undefined || status === '') {
      return { label: '-', className: 'neutral' };
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
          return { label: 'Refunded', className: 'neutral' };
        default:
          return { label: String(status), className: 'neutral' };
      }
    }

    const normalized = status.trim();
    if (!normalized) {
      return { label: '-', className: 'neutral' };
    }

    const key = this.normalizeKey(normalized);
    if (['paid', 'completed', 'success', 'successful'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'good' };
    }

    if (['pending', 'processing', 'awaitingpayment'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'fair' };
    }

    if (['failed', 'cancelled', 'canceled', 'refused'].includes(key)) {
      return { label: this.humanizeStatus(normalized), className: 'low' };
    }

    return { label: this.humanizeStatus(normalized), className: 'neutral' };
  }

  private humanizeStatus(value: string): string {
    return value
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim()
      .replace(/\b\w/g, (char) => char.toUpperCase());
  }

  private tryParseDate(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  private normalizeKey(value: string | number | null | undefined): string {
    return String(value ?? '')
      .trim()
      .toLowerCase()
      .replace(/[\s_-]+/g, '');
  }

  private getReservationStatusKey(status: ReservationStatus | string | number | null | undefined): string {
    const normalized = this.normalizeReservationStatus(status);
    return this.normalizeKey(normalized.label || status);
  }

  private getPaymentStatusKey(status: PaymentStatus | string | number | null | undefined): string {
    const normalized = this.normalizePaymentStatus(status);
    return this.normalizeKey(normalized.label || status);
  }

  private hasReservationStatus(r: GetReservationByIdQueryDto, matches: string[]): boolean {
    return matches.includes(this.getReservationStatusKey(r.status));
  }

  private hasPaymentStatus(r: GetReservationByIdQueryDto, matches: string[]): boolean {
    return matches.includes(this.getPaymentStatusKey(r.paymentStatus));
  }

  private isConfirmedReservation(r: GetReservationByIdQueryDto): boolean {
    return this.hasReservationStatus(r, ['confirmed', 'active', 'approved', 'booked', 'scheduled']);
  }

  private isCashPayment(r: GetReservationByIdQueryDto): boolean {
    return this.normalizeKey(r.paymentTypeName).includes('cash');
  }

  private isDeletableReservation(r: GetReservationByIdQueryDto): boolean {
    return this.hasReservationStatus(r, ['completed', 'cancelled', 'canceled']);
  }

  private isClosedReservation(r: GetReservationByIdQueryDto): boolean {
    return this.hasReservationStatus(r, ['cancelled', 'canceled', 'completed', 'expired', 'past']);
  }
}
