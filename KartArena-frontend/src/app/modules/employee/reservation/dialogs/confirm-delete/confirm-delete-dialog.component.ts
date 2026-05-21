import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDeleteDialogData {
  titleKey?: string;
  messageKey?: string;
  name?: string;
  confirmKey?: string;
  cancelKey?: string;
}

@Component({
  selector: 'app-confirm-delete-dialog',
  templateUrl: './confirm-delete-dialog.component.html',
  styleUrl: './confirm-delete-dialog.component.scss',
  standalone: false,
})
export class ConfirmDeleteDialogReservationComponent {
  constructor(
    private ref: MatDialogRef<ConfirmDeleteDialogReservationComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDeleteDialogData
  ) {}

  cancel(): void {
    this.ref.close(false);
  }

  confirm(): void {
    this.ref.close(true);
  }
}
