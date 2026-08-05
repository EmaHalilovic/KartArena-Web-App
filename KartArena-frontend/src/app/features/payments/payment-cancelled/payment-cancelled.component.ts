import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-payment-cancelled',
  standalone: false,
  templateUrl: './payment-cancelled.component.html',
  styleUrl: './payment-cancelled.component.scss',
})
export class PaymentCancelledComponent {
  private readonly router = inject(Router);

  returnToReservations(): void {
    void this.router.navigate(['/reservations']);
  }
}
