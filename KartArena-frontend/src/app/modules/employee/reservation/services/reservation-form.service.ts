import { GetReservationByIdQueryDto } from './../../../../api-services/reservations/reservation-api.models';
import { Injectable, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';

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

  createReservationCreateForm(): FormGroup {
    return this.fb.group(
      {
        userId: [null, [Validators.min(1)]],
        customerFirstName: ['', [Validators.required, Validators.maxLength(100)]],
        customerLastName: ['', [Validators.required, Validators.maxLength(100)]],
        customerEmail: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
        customerPhone: ['', [Validators.required, Validators.maxLength(50)]],
        customerNote: ['', [Validators.maxLength(500)]],
        trackId: [null, [Validators.required, Validators.min(1)]],
        kartId: [null, [Validators.required, Validators.min(1)]],
        reservationDate: ['', [Validators.required]],
        duration: [10, [Validators.required]],
        startTime: ['', [Validators.required]],
        endTime: ['', [Validators.required]],
        paymentTypeId: [null, [Validators.min(1)]],
        paymentNote: ['', [Validators.maxLength(500)]],
      },
      { validators: this.timeRangeValidator }
    );
  }

  getError(form: FormGroup, controlName: string): string {
    const control = form.get(controlName);
    if (!control || !control.errors) return '';

    const errors = control.errors;

    if (errors['required']) return 'This field is required';
    if (errors['min']) return `Minimum value is ${errors['min'].min}`;
    if (errors['email']) return 'Enter a valid email address';
    if (errors['maxlength']) return `Maximum length is ${errors['maxlength'].requiredLength}`;

    return 'Invalid value';
  }

  private readonly timeRangeValidator = (control: AbstractControl): ValidationErrors | null => {
    const startTime = control.get('startTime')?.value;
    const endTime = control.get('endTime')?.value;
    return startTime && endTime && endTime <= startTime ? { invalidTimeRange: true } : null;
  };
}
