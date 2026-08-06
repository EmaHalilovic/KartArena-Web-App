import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { CreateReservationRequest, GetReservationByIdQueryDto } from '../../../../api-services/reservations/reservation-api.models';
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

  ngOnInit(): void {
    this.initForm(false);
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

  private toIsoDateTime(date: string, time: string): string {
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
