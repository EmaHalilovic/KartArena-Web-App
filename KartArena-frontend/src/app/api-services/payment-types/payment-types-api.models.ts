import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

export class ListPaymentTypesRequest extends BasePagedQuery {
  search?: string | null;
  name?: string | null;
  code?: string | null;
  paymentMethod?: 'online' | 'desk' | null;
  allowedOnline?: boolean | null;
  onlyEnabled?: boolean | null;
}

export interface ListPaymentTypesQueryDto {
  id: number;
  name: string;
  code: string;
  allowedOnline: boolean;
  allowedAtDesk: boolean;
  description?: string | null;
  isEnabled: boolean;
}

export type ListPaymentTypesResponse = PageResult<ListPaymentTypesQueryDto>;

export interface GetPaymentTypeByIdQueryDto {
  id: number;
  name: string;
  code: string;
  allowedOnline: boolean;
  allowedAtDesk: boolean;
  description?: string | null;
  isEnabled: boolean;
}

export interface CreatePaymentTypeCommand {
  name: string;
  code: string;
  allowedOnline: boolean;
  allowedAtDesk: boolean;
  description?: string | null;
}

export interface UpdatePaymentTypeCommand {
  name: string;
  code: string;
  allowedOnline: boolean;
  allowedAtDesk: boolean;
  description?: string | null;
}
