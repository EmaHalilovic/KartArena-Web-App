import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { ToasterService } from '../../../../core/services/toaster.service';

import {
  CreateReservationCommand,
  GetReservationByIdQueryDto,
} from '../../../../api-services/reservations/reservation-api.models';

import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
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

  protected loadData(): void {
    // Add mode: no data to load
  }

  protected override initForm(isEdit: boolean): void {
    super.initForm(isEdit);
    this.form = this.formService.createReservationForm();
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const reservationDate: string = this.form.value.reservationDate; // "YYYY-MM-DD"
    const startTime: string = this.form.value.startTime;             // "HH:mm"
    const endTime: string = this.form.value.endTime;                 // "HH:mm"

    // ✅ backend traži DateTime -> "YYYY-MM-DDTHH:mm:ss"
    const command: CreateReservationCommand = {
      userId: Number(this.form.value.userId),
      trackId: Number(this.form.value.trackId),
      kartId: Number(this.form.value.kartId),
      reservationDate: reservationDate,
      startTime: `${reservationDate}T${startTime}:00`,
      endTime: `${reservationDate}T${endTime}:00`,
      amount: Number(this.form.value.amount ?? 0),
      paymentTypeId: Number(this.form.value.paymentTypeId ?? 0),
      paymentNote: this.form.value.paymentNote?.trim() || null,
    };

    // ✅ backend traži wrapper: { command: ... }
    this.api.create(command as any).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Reservation created successfully');
        this.router.navigate(['/client/reservation']);
      },
      error: (err) => {
        this.stopLoading('Failed to create reservation');
        console.error('Create reservation error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/client/reservation']);
  }

  getErrorMessage(controlName: string): string {
    // uskladi ime metode sa tvojim form service-om
    // ako imaš getError(...) umjesto getErrorMessage(...), promijeni ovdje:
    return this.formService.getError(this.form, controlName);
  }

  private toIsoDateTime(date: string, time: string): string {
    // time može biti "HH:mm" ili "HH:mm:ss"
    if (!date || !time) return '';
    const t = /^\d{2}:\d{2}$/.test(time) ? `${time}:00` : time;
    return `${date}T${t}`;
  }
}
