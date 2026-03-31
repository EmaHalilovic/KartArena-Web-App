import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

export enum PaymentStatus {
  Pending = 0,
  Paid = 1,
  Failed = 2,
  Refunded = 3,
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
  reservationDate?: string | null;
  amount: number;
  paymentDate?: string | null;
  paymentTypeName?: string | null;
  status: PaymentStatus;
}

export type ListPaymentsResponse = PageResult<ListPaymentsQueryDto>;

export interface GetPaymentByIdQueryDto {
  id: number;
  reservationId: number;
  amount: number;
  paymentDate?: string | null;
  paymentTypeId?: number | null;
  paymentTypeName?: string | null;
  status: PaymentStatus;
  transactionReference?: string | null;
  note?: string | null;
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
