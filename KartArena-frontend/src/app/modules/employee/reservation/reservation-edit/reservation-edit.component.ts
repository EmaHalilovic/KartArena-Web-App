import { Component, OnInit, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import {
  GetReservationByIdQueryDto,
  ReservationStatus,
  UpdateReservationCommand,
} from '../../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { ReservationFormService } from '../services/reservation-form.service';

@Component({
  selector: 'app-employee-reservation-edit',
  standalone: false,
  templateUrl: '../../../admin/reservation/reservation-edit/reservation-edit.component.html',
  styleUrl: '../../../admin/reservation/reservation-edit/reservation-edit.component.scss',
  providers: [ReservationFormService],
})
export class ReservationEditComponent implements OnInit {
  private readonly api = inject(ReservationApiService);
  private readonly formService = inject(ReservationFormService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  form!: FormGroup;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  reservation: GetReservationByIdQueryDto | null = null;
  readonly minDate = new Date();
  id!: number;

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.form = this.formService.createReservationForm();
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.api.getById(this.id).subscribe({
      next: (dto) => {
        if (dto.status !== ReservationStatus.Confirmed) {
          this.isLoading = false;
          this.toaster.error('Only confirmed reservations can be edited');
          this.router.navigate(['/employee/reservations', this.id]);
          return;
        }

        this.reservation = dto;
        this.isLoading = false;
        this.form = this.formService.createReservationForm(dto);
        this.form.get('userId')?.clearValidators();
        this.form.get('userId')?.updateValueAndValidity();
        this.form.patchValue({
          reservationDate: this.toLocalDate(dto.date),
          startTime: this.toTimeInput(dto.startTime),
          endTime: this.toTimeInput(dto.endTime),
        });
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Reservation could not be loaded.';
        console.error('Get reservation error:', err);
        this.toaster.error('Reservation not found');
        this.router.navigate(['/employee/reservations']);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/employee/reservations']);
  }

  onSubmit(): void {
    this.errorMessage = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const reservationDate = this.toIsoDate(value.reservationDate);
    const startTime = this.toTimeInput(value.startTime);
    const endTime = this.toTimeInput(value.endTime);

    if (!reservationDate || !startTime || !endTime || endTime <= startTime) {
      this.errorMessage = 'End time must be after start time.';
      return;
    }

    const command: UpdateReservationCommand = {
      userId: value.userId == null ? null : Number(value.userId),
      trackId: Number(value.trackId),
      kartId: Number(value.kartId),
      reservationDate,
      startTime: `${reservationDate}T${startTime}:00`,
      endTime: `${reservationDate}T${endTime}:00`,
    };

    this.isSaving = true;
    this.api.update(this.id, command).subscribe({
      next: () => {
        this.isSaving = false;
        this.toaster.success('Reservation updated');
        this.router.navigate(['/employee/reservations', this.id]);
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = this.readApiError(err) || 'Update reservation failed.';
        console.error('Update reservation error:', err);
        this.toaster.error(this.errorMessage);
      },
    });
  }

  err(name: string): string {
    return this.formService.getError(this.form, name);
  }

  get reservationCode(): string {
    return `RES-${String(this.id).padStart(4, '0')}`;
  }

  get customerLabel(): string {
    const firstName = this.reservation?.customerFirstName ?? this.reservation?.userFirstName ?? '';
    const lastName = this.reservation?.customerLastName ?? this.reservation?.userLastName ?? '';
    return `${firstName} ${lastName}`.trim() || `User #${this.reservation?.userId ?? '-'}`;
  }

  get trackLabel(): string {
    return this.reservation?.trackName?.trim() || `Track #${this.reservation?.trackId ?? '-'}`;
  }

  get kartLabel(): string {
    return this.reservation?.kartName?.trim() || `Kart #${this.reservation?.kartId ?? '-'}`;
  }

  private toLocalDate(value: string): Date | null {
    const datePart = value?.split('T')[0];
    if (!datePart) return null;
    const [year, month, day] = datePart.split('-').map(Number);
    return year && month && day ? new Date(year, month - 1, day) : null;
  }

  private toIsoDate(value: Date | string | null): string {
    if (!value) return '';
    if (typeof value === 'string') return value.split('T')[0];
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toTimeInput(value?: string | null): string {
    if (!value) return '';
    const time = value.includes('T') ? value.split('T')[1] ?? value : value;
    return time.substring(0, 5);
  }

  private readApiError(error: any): string {
    const body = error?.error;
    if (typeof body === 'string') return body;
    return body?.detail || body?.title || body?.message || error?.message || '';
  }
}
