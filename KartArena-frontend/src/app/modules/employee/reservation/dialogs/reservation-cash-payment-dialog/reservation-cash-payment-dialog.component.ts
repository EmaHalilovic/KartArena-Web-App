import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { MarkReservationCashPaidPayload } from '../../../../../api-services/reservations/reservation-api.models';

export interface ReservationCashPaymentDialogData {
  reservationCode: string;
  customerName: string;
  amount?: number | null;
}

@Component({
  selector: 'app-reservation-cash-payment-dialog',
  templateUrl: './reservation-cash-payment-dialog.component.html',
  styleUrl: './reservation-cash-payment-dialog.component.scss',
  standalone: false,
})
export class ReservationCashPaymentDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    transactionReference: [''],
    note: ['', [Validators.maxLength(250)]],
  });

  constructor(
    private readonly ref: MatDialogRef<ReservationCashPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public readonly data: ReservationCashPaymentDialogData
  ) {}

  cancel(): void {
    this.ref.close();
  }

  confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: MarkReservationCashPaidPayload = {
      transactionReference: this.form.value.transactionReference?.trim() || null,
      note: this.form.value.note?.trim() || null,
    };

    this.ref.close(payload);
  }
}
