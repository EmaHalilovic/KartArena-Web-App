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

  private readonly baseUrl = `${environment.apiUrl}/payments/controller`;

  createCheckoutSession(
    paymentId: number
  ): Observable<CreateCheckoutSessionResponse> {
    const request: CreateCheckoutSessionRequest = {
      paymentId
    };

    return this.http.post<CreateCheckoutSessionResponse>(
      `${this.baseUrl}/checkout-session`,
      request
    );
  }
}
