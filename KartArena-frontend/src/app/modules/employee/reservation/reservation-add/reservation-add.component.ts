import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { ListPaymentTypesQueryDto, ListPaymentTypesRequest } from '../../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../../api-services/payment-types/payment-types-api.service';
import { AvailableKartDto, AvailableTimeDto, AvailableTrackDto, CreateReservationRequest, GetReservationByIdQueryDto } from '../../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { ReservationFormService } from '../services/reservation-form.service';

@Component({
  selector: 'app-reservation-add',
  standalone: false,
  templateUrl: './reservation-add.component.html',
  styleUrl: './reservation-add.component.scss',
  providers: [ReservationFormService],
})
export class ReservationAddComponent
  extends BaseFormComponent<GetReservationByIdQueryDto>
  implements OnInit
{
  private api = inject(ReservationApiService);
  private formService = inject(ReservationFormService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private paymentTypesApi = inject(PaymentTypesApiService);

  readonly durationOptions = [10, 15];
  availability: { availableTimes: AvailableTimeDto[] } | null = null;
  startTimeOptions: string[] = [];
  endTimeOptions: string[] = [];
  trackOptions: AvailableTrackDto[] = [];
  kartOptions: AvailableKartDto[] = [];
  paymentTypeOptions: ListPaymentTypesQueryDto[] = [];
  isLoadingOptions = false;

  ngOnInit(): void {
    this.initForm(false);
    this.bindOptionChanges();
    this.loadPaymentTypes();
  }

  protected loadData(): void {}

  protected override initForm(isEdit: boolean): void {
    super.initForm(isEdit);
    this.form = this.formService.createReservationCreateForm();
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const reservationDate = this.toLocalIsoDate(value.reservationDate);
    const request: CreateReservationRequest = {
      reservationDate,
      startTime: this.toIsoDateTime(reservationDate, value.startTime),
      endTime: this.toIsoDateTime(reservationDate, value.endTime),
      userId: this.toNullableNumber(value.userId),
      customerFirstName: value.customerFirstName.trim(),
      customerLastName: value.customerLastName.trim(),
      customerEmail: value.customerEmail.trim(),
      customerPhone: value.customerPhone.trim(),
      customerNote: value.customerNote?.trim() || null,
      kartId: Number(value.kartId),
      trackId: Number(value.trackId),
      paymentTypeId: this.toNullableNumber(value.paymentTypeId),
      paymentNote: value.paymentNote?.trim() || null,
    };

    this.startLoading();
    this.api.create(request).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Reservation created successfully');
        this.router.navigate(['/employee/reservations']);
      },
      error: (err) => {
        this.stopLoading('Failed to create reservation');
        this.toaster.error(err?.error?.message ?? 'Failed to create reservation');
        console.error('Create reservation error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/employee/reservations']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getError(this.form, controlName);
  }

  formatTime(value: string): string {
    const time = value.includes('T') ? value.split('T')[1] : value;
    return time?.split('.')[0].slice(0, 5);
  }

  private bindOptionChanges(): void {
    this.form.get('reservationDate')?.valueChanges.subscribe(() => this.loadAvailability());
    this.form.get('duration')?.valueChanges.subscribe(() => this.loadAvailability());
    this.form.get('startTime')?.valueChanges.subscribe(() => this.applySelectedTime());
    this.form.get('trackId')?.valueChanges.subscribe((trackId) => this.applySelectedTrack(trackId));
  }

  private loadAvailability(): void {
    const dateValue = this.form.get('reservationDate')?.value;
    const duration = Number(this.form.get('duration')?.value);

    this.form.patchValue(
      { startTime: '', endTime: '', trackId: null, kartId: null },
      { emitEvent: false }
    );
    this.availability = null;
    this.startTimeOptions = [];
    this.endTimeOptions = [];
    this.trackOptions = [];
    this.kartOptions = [];

    if (!dateValue || !this.durationOptions.includes(duration)) {
      return;
    }

    const date = this.toLocalIsoDate(dateValue);
    this.isLoadingOptions = true;
    this.api.getAvailability(date, duration)
      .pipe(finalize(() => (this.isLoadingOptions = false)))
      .subscribe({
        next: (availability) => {
          this.availability = availability;
          this.startTimeOptions = availability.availableTimes.map((slot) => slot.startTime);
        },
        error: (err) => {
          console.error('Load reservation availability error:', err);
          this.toaster.error('Available reservation options could not be loaded');
        },
      });
  }

  private applySelectedTime(): void {
    const slot = this.getSelectedSlot();
    this.form.patchValue(
      {
        endTime: slot?.endTime ?? '',
        trackId: null,
        kartId: null,
      },
      { emitEvent: false }
    );
    this.trackOptions = slot?.tracks ?? [];
    this.endTimeOptions = slot ? [slot.endTime] : [];
    this.kartOptions = [];
  }

  private applySelectedTrack(trackId: number | null): void {
    const slot = this.getSelectedSlot();
    const track = slot?.tracks.find((item) => item.trackId === Number(trackId));
    this.form.patchValue({ kartId: null }, { emitEvent: false });
    this.kartOptions = track?.availableKarts ?? [];
  }

  private getSelectedSlot(): AvailableTimeDto | null {
    const startTime = this.form.get('startTime')?.value;
    return this.availability?.availableTimes.find((slot) => slot.startTime === startTime) ?? null;
  }

  private loadPaymentTypes(): void {
    const request = new ListPaymentTypesRequest();
    request.onlyEnabled = true;
    request.paging.pageSize = 1000;

    this.paymentTypesApi.list(request).subscribe({
      next: (response) => {
        this.paymentTypeOptions = response.items.filter((item) =>
          item.code?.trim().toUpperCase() === 'CASH' ||
          item.name?.trim().toLowerCase().includes('cash')
        );

        if (this.paymentTypeOptions.length) {
          this.form.patchValue({ paymentTypeId: this.paymentTypeOptions[0].id });
        }
      },
      error: (err) => {
        console.error('Load payment types error:', err);
        this.paymentTypeOptions = [];
      },
    });
  }

  private toIsoDateTime(date: string, time: string): string {
    if (time.includes('T')) return time;
    const normalizedTime = /^\d{2}:\d{2}$/.test(time) ? `${time}:00` : time;
    return `${date}T${normalizedTime}`;
  }

  private toLocalIsoDate(value: string | Date): string {
    if (typeof value === 'string') {
      return value.includes('T') ? value.split('T')[0] : value;
    }

    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toNullableNumber(value: unknown): number | null {
    return value === null || value === undefined || value === '' ? null : Number(value);
  }
}
