import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ReservationActionConfirmDialogData {
  title: string;
  message: string;
  confirmText: string;
  icon?: string | null;
}

@Component({
  selector: 'app-reservation-action-confirm-dialog',
  templateUrl: './reservation-action-confirm-dialog.component.html',
  styleUrl: './reservation-action-confirm-dialog.component.scss',
  standalone: false,
})
export class ReservationActionConfirmDialogComponent {
  constructor(
    private readonly ref: MatDialogRef<ReservationActionConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public readonly data: ReservationActionConfirmDialogData
  ) {}

  cancel(): void {
    this.ref.close(false);
  }

  confirm(): void {
    this.ref.close(true);
  }
}
