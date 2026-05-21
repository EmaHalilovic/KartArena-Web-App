import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDeletePaymentTypeDialogData {
  titleKey?: string;
  messageKey?: string;
  name?: string;
  confirmKey?: string;
  cancelKey?: string;
}

@Component({
  selector: 'app-confirm-delete-payment-type-dialog',
  templateUrl: './confirm-delete-dialog.component.html',
  styleUrl: './confirm-delete-dialog.component.scss',
  standalone: false,
})
export class ConfirmDeletePaymentTypeDialogComponent {
  constructor(
    private ref: MatDialogRef<ConfirmDeletePaymentTypeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDeletePaymentTypeDialogData
  ) {}

  cancel(): void {
    this.ref.close(false);
  }

  confirm(): void {
    this.ref.close(true);
  }
}
