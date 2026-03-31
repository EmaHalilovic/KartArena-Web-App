import { Component, inject, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { ToasterService } from '../../../../core/services/toaster.service';
import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
import { ReservationFormService } from '../services/reservation-form.service';
import { UpdateReservationCommand } from '../../../../api-services/reservations/reservation-api.models';

@Component({
  selector: 'app-reservation-edit',
  standalone: false,
  templateUrl: './reservation-edit.component.html',
  styleUrl: './reservation-edit.component.scss',
  providers: [ReservationFormService],
})
export class ReservationEditComponent implements OnInit {
  private api = inject(ReservationApiService);
  private formService = inject(ReservationFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  form!: FormGroup;
  isLoading = false;
  isSaving = false;
  id!: number;

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.form = this.formService.createReservationForm();
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.getById(this.id).subscribe({
      next: (dto) => {
        this.isLoading = false;
        this.form = this.formService.createReservationForm(dto);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Get reservation error:', err);
        this.toaster.error('Reservation not found');
        this.router.navigate(['/client/reservation']);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/client/reservation']);
  }

 onSubmit(): void {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }

  this.isSaving = true;

  const raw = this.form.getRawValue() as any;

  // reservationDate može biti Date ili string
  const date = this.toShortIsoDate(raw.reservationDate);

  // start/end iz input type="time" dolaze kao "HH:mm"
  const command: UpdateReservationCommand = {
    userId: Number(raw.userId),
    trackId: Number(raw.trackId),
    kartId: Number(raw.kartId),
    reservationDate: date as any,
    startTime: this.toIsoLocalDateTime(date, raw.startTime) as any,
    endTime: this.toIsoLocalDateTime(date, raw.endTime) as any,
  };

  

  this.api.update(this.id, command as any).subscribe({
    next: () => {
      this.isSaving = false;
      this.toaster.success('Reservation updated');
      this.router.navigate(['/client/reservation']);
    },
    error: (err) => {
      this.isSaving = false;
      console.error('Update reservation error:', err?.error ?? err);
      this.toaster.error('Update reservation failed');
    },
  });
}

  err(name: string): string {
    return this.formService.getError(this.form, name);
  }

  private toShortIsoDate(date: string | Date): string {
  if (!date) return '';

  if (date instanceof Date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  return String(date).includes('T') ? String(date).split('T')[0] : String(date);
}

private toIsoLocalDateTime(date: string, time: string): string {
  if (!date || !time) return '';
  const t = /^\d{2}:\d{2}$/.test(time) ? `${time}:00` : time; // "HH:mm" -> "HH:mm:00"
  return `${date}T${t}`;
}

}
