import { GetReservationByIdQueryDto } from './../../../../api-services/reservations/reservation-api.models';
import { Injectable, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Injectable()
export class ReservationFormService {
  private fb = inject(FormBuilder);

  createReservationForm(res?: GetReservationByIdQueryDto): FormGroup {
    return this.fb.group({
      userId: [res?.userId ?? null, [Validators.required, Validators.min(1)]],
      trackId: [res?.trackId ?? null, [Validators.required, Validators.min(1)]],
      kartId: [res?.kartId ?? null, [Validators.required, Validators.min(1)]],
      reservationDate: [res?.date ?? '', [Validators.required]],
      startTime: [res?.startTime ?? '', [Validators.required]],
      endTime: [res?.endTime ?? '', [Validators.required]],
    });
  }

  getError(form: FormGroup, controlName: string): string {
    const control = form.get(controlName);
    if (!control || !control.errors) return '';

    const errors = control.errors;

    if (errors['required']) return 'This field is required';
    if (errors['min']) return `Minimum value is ${errors['min'].min}`;

    return 'Invalid value';
  }
}
