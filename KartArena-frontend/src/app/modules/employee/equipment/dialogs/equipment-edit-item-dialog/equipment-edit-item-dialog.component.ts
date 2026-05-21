import { Component, Inject, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { GetEquipmentByIdQueryDtoItem } from '../../../../../api-services/equipment/equipment-api.models';
import { UpdateEquipmentItemCommand } from '../../../../../api-services/equipment items/equipmentItem-api.models';
import { EquipmentFormService } from '../../services/equipment-form.service';

interface EquipmentEditItemDialogData {
  equipmentName: string;
  item: GetEquipmentByIdQueryDtoItem;
}

@Component({
  selector: 'app-equipment-edit-item-dialog',
  standalone: false,
  templateUrl: './equipment-edit-item-dialog.component.html',
  styleUrl: './equipment-edit-item-dialog.component.scss',
  providers: [EquipmentFormService],
})
export class EquipmentEditItemDialogComponent {
  private formService = inject(EquipmentFormService);

  form: FormGroup;

  constructor(
    private ref: MatDialogRef<EquipmentEditItemDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EquipmentEditItemDialogData
  ) {
    this.form = this.formService.createEquipmentItemForm();
    this.form.patchValue({
      itemCode: data.item.itemCode,
      status: data.item.status,
      purchaseDate: this.toDate(data.item.purchaseDate),
      lastInspectedAt: this.toDate(data.item.lastInspectedAt),
      notes: data.item.notes ?? '',
    });
  }

  close(): void {
    this.ref.close();
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const payload: UpdateEquipmentItemCommand = {
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

  private toDate(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  private toIsoDate(value: unknown): string | null {
    if (!value) {
      return null;
    }

    const date = value instanceof Date ? value : new Date(String(value));
    return Number.isNaN(date.getTime()) ? null : date.toISOString();
  }
}
