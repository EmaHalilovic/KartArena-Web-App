import { Component, Inject, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { AddEquipmentItemCommand } from '../../../../../api-services/equipment/equipment-api.models';
import { EquipmentFormService } from '../../services/equipment-form.service';

interface EquipmentAddItemDialogData {
  equipmentName: string;
}

@Component({
  selector: 'app-equipment-add-item-dialog',
  standalone: false,
  templateUrl: './equipment-add-item-dialog.component.html',
  styleUrl: './equipment-add-item-dialog.component.scss',
  providers: [EquipmentFormService],
})
export class EquipmentAddItemDialogComponent {
  private formService = inject(EquipmentFormService);

  form: FormGroup = this.formService.createEquipmentItemForm();

  constructor(
    private ref: MatDialogRef<EquipmentAddItemDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EquipmentAddItemDialogData
  ) {}

  close(): void {
    this.ref.close();
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const payload: AddEquipmentItemCommand = {
      itemCode: value.itemCode,
      status: value.status,
      purchaseDate: this.toIsoDate(value.purchaseDate),
      lastInspectedAt: this.toIsoDate(value.lastInspectedAt),
      notes: value.notes || null,
    };

    this.ref.close(payload);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }

  private toIsoDate(value: unknown): string | null {
    if (!value) {
      return null;
    }

    const date = value instanceof Date ? value : new Date(String(value));
    return Number.isNaN(date.getTime()) ? null : date.toISOString();
  }
}
