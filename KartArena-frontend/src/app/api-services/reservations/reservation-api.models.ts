import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PaymentStatus } from '../payments/payments-api.models';

export enum ReservationStatus {
  Pending = 1,
  Confirmed = 2,
  Completed = 3,
  Cancelled = 4,
}

export class ListReservationRequest extends BasePagedQuery {
  search?: string | null;
  userId?: number | null;
  trackId?: number | null;
  kartId?: number | null;
  status?: ReservationStatus | null;
  paymentStatus?: PaymentStatus | null;
  paymentTypeName?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface ListReservationQueryDto {
  id: number;
  userId: number;
  trackId: number;
  kartId: number;
  date: string;
  startTime: string;
  endTime: string;
  status?: ReservationStatus | null;
  paymentStatus?: PaymentStatus | null;
  paymentAmount?: number | null;
  paymentTypeName?: string | null;
  paymentDate?: string | null;
  transactionReference?: string | null;
  paymentNote?: string | null;
  assignedEmployeeId?: number | null;
  assignedEmployeeName?: string | null;
  assignedEquipmentItemId?: number | null;
  assignedEquipmentItemCode?: string | null;
  assignedEquipmentItemName?: string | null;
  userFirstName?: string | null;
  userLastName?: string | null;
  trackName?: string | null;
  kartName?: string | null;
}

export type ListReservationResponse = PageResult<ListReservationQueryDto>;

export interface GetReservationByIdQueryDto {
  id: number;
  userId: number;
  trackId: number;
  kartId: number;
  date: string;
  startTime: string;
  endTime: string;
  status?: ReservationStatus | null;
  paymentStatus?: PaymentStatus | null;
  paymentAmount?: number | null;
  paymentTypeName?: string | null;
  paymentDate?: string | null;
  transactionReference?: string | null;
  paymentNote?: string | null;
  assignedEmployeeId?: number | null;
  assignedEmployeeName?: string | null;
  assignedEquipmentItemId?: number | null;
  assignedEquipmentItemCode?: string | null;
  assignedEquipmentItemName?: string | null;
  userFirstName?: string | null;
  userLastName?: string | null;
  trackName?: string | null;
  kartName?: string | null;
  createdAt?: string | null;
  modifiedAt?: string | null;
}

export interface CreateReservationCommand {
  userId: number;
  trackId: number;
  kartId: number;
  reservationDate: string;
  startTime: string;
  endTime: string;
  amount: number;
  paymentTypeId: number;
  paymentNote?: string | null;
}

export interface UpdateReservationCommand {
  userId?: number | null;
  trackId?: number | null;
  kartId?: number | null;
  reservationDate?: string | null;
  startTime?: string | null;
  endTime?: string | null;
}

export interface MarkReservationCashPaidPayload {
  transactionReference?: string | null;
  note?: string | null;
}

export interface MarkReservationCashPaidCommand {
  reservationId: number;
  transactionReference?: string | null;
  note?: string | null;
}

export interface AssignReservationResourcesPayload {
  employeeId?: number | null;
  employeeName?: string | null;
  equipmentItemIds: number[];
  replaceExistingAssignments?: boolean;
}

export interface AssignReservationResourcesCommand {
  reservationId: number;
  employeeId?: number | null;
  employeeName?: string | null;
  equipmentItemIds: number[];
  replaceExistingAssignments: boolean;
}

export interface ReservationEmployeeAssignmentDto {
  id: number;
  reservationId: number;
  employeeId: number;
  employeeName: string;
  equipmentItemId?: number | null;
  equipmentItemName?: string | null;
  equipmentTypeId?: number | null;
  equipmentCategoryName?: string | null;
  createdAtUtc: string;
}

export interface AvailableReservationEquipmentItemDto {
  id: number;
  itemCode: string;
  equipmentTypeId: number;
  equipmentTypeName: string;
}
