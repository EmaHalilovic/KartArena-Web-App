import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

export enum PaymentStatus {
  Pending = 0,
  Paid = 1,
  Failed = 2,
  Cancelled = 3,
  Refunded = 4
}

export class ListPaymentsRequest extends BasePagedQuery {
  search?: string | null;
  reservationId?: number | null;
  paymentTypeId?: number | null;
  status?: PaymentStatus | null;
}

export interface ListPaymentsQueryDto {
  id: number;
  customerName?: string | null;
  reservations: ListPaymentReservationDto[];
  amount: number;
  currency: string;
  paymentDate?: string | null;
  paymentTypeName?: string | null;
  status: PaymentStatus;
}

export interface ListPaymentReservationDto {
  id: number;
  date: string;
  startTime: string;
  endTime: string;
  trackName: string;
  kartName: string;
}

export type ListPaymentsResponse = PageResult<ListPaymentsQueryDto>;

export interface GetPaymentByIdQueryDto {
  id: number;
  reservations: GetPaymentReservationDto[];
  amount: number;
  currency: string;
  paymentDate?: string | null;
  paymentTypeId?: number | null;
  paymentTypeName?: string | null;
  status: PaymentStatus;
  transactionReference?: string | null;
  note?: string | null;
}

export interface GetPaymentReservationDto {
  id: number;
  customerName: string;
  customerEmail: string;
  date: string;
  startTime: string;
  endTime: string;
  trackName: string;
  kartName: string;
  totalPrice: number;
}

export interface CreatePaymentCommand {
  reservationId: number;
  amount: number;
  paymentDate?: string | null;
  paymentTypeId?: number | null;
  transactionReference?: string | null;
  note?: string | null;
}

export interface UpdatePaymentCommand {
  amount: number;
  paymentDate?: string | null;
  paymentTypeId?: number | null;
  transactionReference?: string | null;
  note?: string | null;
}
