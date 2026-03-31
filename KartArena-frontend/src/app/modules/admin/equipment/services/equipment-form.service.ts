import { inject, Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
  EquipmentCategory,
  EquipmentItemStatus,
  EquipmentTypeDetailsDto,
} from '../../../../api-services/equipment/equipment-api.models';

@Injectable()
export class EquipmentFormService {
  private fb = inject(FormBuilder);
  private readonly categoryOptions = [
    EquipmentCategory.Helmet,
    EquipmentCategory.Suit,
    EquipmentCategory.Gloves,
    EquipmentCategory.Balaclava,
    EquipmentCategory.Other,
  ] as const;

  createEquipmentTypeForm(equipment?: EquipmentTypeDetailsDto): FormGroup {
    return this.fb.group({
      name: [
        equipment?.name ?? '',
        [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
      ],
      category: [
        this.toCategoryOptionValue(equipment?.category),
        [Validators.required],
      ],
      size: [
        equipment?.size ?? '',
        [Validators.required, Validators.minLength(1), Validators.maxLength(50)],
      ],
      description: [equipment?.description ?? '', [Validators.maxLength(500)]],
      price: [
        equipment?.price ?? 0,
        [Validators.required, Validators.min(0.01), Validators.max(1000000)],
      ],
    });
  }

  createEquipmentItemForm(): FormGroup {
    return this.fb.group({
      itemCode: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      status: [EquipmentItemStatus.Available, [Validators.required]],
      purchaseDate: [''],
      lastInspectedAt: [''],
      notes: ['', [Validators.maxLength(500)]],
    });
  }

  createBulkEquipmentItemsForm(): FormGroup {
    return this.fb.group({
      prefix: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20)]],
      startNumber: [1, [Validators.required, Validators.min(1), Validators.max(1000000)]],
      count: [1, [Validators.required, Validators.min(1), Validators.max(1000)]],
      numberPadding: [3, [Validators.required, Validators.min(1), Validators.max(10)]],
      status: [EquipmentItemStatus.Available, [Validators.required]],
      purchaseDate: [''],
      lastInspectedAt: [''],
      notes: ['', [Validators.maxLength(500)]],
    });
  }

  toCategoryOptionValue(category?: EquipmentCategory | null): number {
    if (!category) {
      return 0;
    }

    const index = this.categoryOptions.indexOf(category);
    return index >= 0 ? index : 0;
  }

  toCategoryEnum(value: unknown): EquipmentCategory {
    if (typeof value === 'number' && this.categoryOptions[value]) {
      return this.categoryOptions[value];
    }

    if (typeof value === 'string') {
      const numericValue = Number(value);
      if (!Number.isNaN(numericValue) && this.categoryOptions[numericValue]) {
        return this.categoryOptions[numericValue];
      }

      const matchedCategory = this.categoryOptions.find((category) => category === value);
      if (matchedCategory) {
        return matchedCategory;
      }
    }

    return EquipmentCategory.Helmet;
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
    if (errors['min']) return `Minimum value is ${errors['min'].min}`;
    if (errors['max']) return `Maximum value is ${errors['max'].max}`;

    return 'Invalid value';
  }
}
