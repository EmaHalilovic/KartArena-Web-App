import { inject, Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { GetPaymentTypeByIdQueryDto } from '../../../../api-services/payment-types/payment-types-api.models';

@Injectable()
export class PaymentTypesFormService {
  private fb = inject(FormBuilder);

  createPaymentTypeForm(paymentType?: GetPaymentTypeByIdQueryDto): FormGroup {
    return this.fb.group({
      name: [
        paymentType?.name ?? '',
        [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
      ],
      code: [
        paymentType?.code ?? '',
        [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
      ],
      allowedOnline: [paymentType?.allowedOnline ?? false],
      allowedAtDesk: [paymentType?.allowedAtDesk ?? true],
      description: [paymentType?.description ?? '', [Validators.maxLength(500)]],
      
    });
  }

  getErrorMessage(form: FormGroup, controlName: string): string {
    const control = form.get(controlName);
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    const errors = control.errors;

    if (errors['required']) return 'This field is required';
    if (errors['minlength']) return `Minimum ${errors['minlength'].requiredLength} characters required`;
    if (errors['maxlength']) return `Maximum ${errors['maxlength'].requiredLength} characters allowed`;

    return 'Invalid value';
  }
}
