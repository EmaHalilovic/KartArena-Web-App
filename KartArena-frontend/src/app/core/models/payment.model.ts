export interface CreateCheckoutSessionRequest {
  reservationId: number;
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
  sessionId: string;
  reservationId: number;
}