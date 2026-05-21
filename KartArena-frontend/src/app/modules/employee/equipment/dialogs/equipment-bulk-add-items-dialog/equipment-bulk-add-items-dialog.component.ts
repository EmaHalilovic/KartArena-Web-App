import { Component, Inject, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { BulkAddEquipmentItemsCommand } from '../../../../../api-services/equipment/equipment-api.models';
import { EquipmentFormService } from '../../services/equipment-form.service';

interface EquipmentBulkAddItemsDialogData {
  equipmentName: string;
  equipmentSize: string;
}

@Component({
  selector: 'app-equipment-bulk-add-items-dialog',
  standalone: false,
  templateUrl: './equipment-bulk-add-items-dialog.component.html',
  styleUrl: './equipment-bulk-add-items-dialog.component.scss',
  providers: [EquipmentFormService],
})
export class EquipmentBulkAddItemsDialogComponent {
  private formService = inject(EquipmentFormService);

  form: FormGroup = this.formService.createBulkEquipmentItemsForm();

  constructor(
    private ref: MatDialogRef<EquipmentBulkAddItemsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EquipmentBulkAddItemsDialogData
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
    const payload: BulkAddEquipmentItemsCommand = {
      prefix: value.prefix,
      startNumber: value.startNumber,
      count: value.count,
      numberPadding: value.numberPadding,
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
