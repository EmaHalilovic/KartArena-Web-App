import { Injectable, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GetKartByIdQueryDto } from '../../../../api-services/karts/karts-api.models';

@Injectable()
export class KartsFormService {
  private fb = inject(FormBuilder);

  createKartForm(kart?: GetKartByIdQueryDto): FormGroup {
    return this.fb.group({
      name: [kart?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      manufacturer: [kart?.manufacturer ?? '', [Validators.maxLength(100)]],
      colour: [kart?.colour ?? '', [Validators.maxLength(50)]],
      yearOfManufacture: [kart?.yearOfManufacture ?? null],
      chassisNumber: [kart?.chassisNumber ?? '', [Validators.required, Validators.maxLength(50)]],
      powertrainTypeId: [kart?.powertrainTypeId ?? null],

      imageUrl: [kart?.imageUrl ?? '', [Validators.maxLength(500)]],
      pricePerSession: [kart?.pricePerSession ?? null, [Validators.min(0), Validators.max(99999)]],
      description: [kart?.description ?? '', [Validators.maxLength(2000)]],
    });
  }

  getErrorMessage(form: FormGroup, controlName: string): string {
    const control = form.get(controlName);
    if (!control || !control.errors || !control.touched) return '';

    const errors = control.errors;
    if (errors['required']) return 'This field is required';
    if (errors['minlength']) return `Minimum ${errors['minlength'].requiredLength} characters required`;
    if (errors['maxlength']) return `Maximum ${errors['maxlength'].requiredLength} characters allowed`;
    if (errors['min']) return `Minimum value is ${errors['min'].min}`;
    if (errors['max']) return `Maximum value is ${errors['max'].max}`;

    return 'Invalid value';
  }
}
