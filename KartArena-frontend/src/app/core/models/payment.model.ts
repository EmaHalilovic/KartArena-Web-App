export interface CreateCheckoutSessionRequest {
  paymentId: number;
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
  sessionId: string;
  paymentId: number;
  reservationIds: number[];
}
