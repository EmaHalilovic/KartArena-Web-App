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
  if (this.form.invalid || this.isLoading) return;

  this.startLoading();

 const rawDate = this.form.value.reservationDate; // Date
const date = this.toShortIsoDate(rawDate);

const startTime: string = this.form.value.startTime; // "HH:mm"
const endTime: string = this.form.value.endTime;     // "HH:mm"

const command: CreateReservationCommand = {
  userId: Number(this.form.value.userId),
  trackId: Number(this.form.value.trackId),
  kartId: Number(this.form.value.kartId),
  reservationDate: date,
  startTime: this.toIsoLocalDateTime(date, startTime),
  endTime: this.toIsoLocalDateTime(date, endTime),
};


  this.api.create(command).subscribe({
    next: () => {
      this.stopLoading();
      this.toaster.success('Reservation created successfully');
      this.router.navigate(['/client/reservation']);
    },
    error: (err) => {
      console.error('Create reservation error:', err);
      console.error('Server body:', err?.error);
      this.stopLoading('Failed to create reservation');
    },
  });
}

private toShortIsoDate(date: string | Date): string {
  if (!date) return '';

  // Date object from mat-datepicker
  if (date instanceof Date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  // string (maybe ISO)
  return date.includes('T') ? date.split('T')[0] : date;
}

private toIsoLocalDateTime(date: string, time: string): string {
  if (!date || !time) return '';

  const t = /^\d{2}:\d{2}$/.test(time) ? `${time}:00` : time; // "HH:mm" -> "HH:mm:00"
  return `${date}T${t}`; // "YYYY-MM-DDTHH:mm:ss"
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
