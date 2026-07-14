import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse
} from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl =
    `${environment.apiUrl}/payments`;

  createCheckoutSession(
    reservationId: number
  ): Observable<CreateCheckoutSessionResponse> {
    const request: CreateCheckoutSessionRequest = {
      reservationId
    };

    return this.http.post<CreateCheckoutSessionResponse>(
      `${this.baseUrl}/checkout-session`,
      request
    );
  }
}