import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

export class ListReservationRequest extends BasePagedQuery {
  search?: string | null;

  // Optional filteri (ako ih budeš imao na backendu)
  userId?: number | null;
  trackId?: number | null;
  kartId?: number | null;

  // Ako želiš filtrirati po datumu
  fromDate?: string | null; // "YYYY-MM-DD"
  toDate?: string | null;   // "YYYY-MM-DD"
}

export interface ListReservationQueryDto {
  id: number;

  userId: number;
  trackId: number;
  kartId: number;

  date: string;       // ISO string npr. "2026-01-15T00:00:00"
  startTime: string;  // npr. "10:00:00" ili ISO ovisno kako vraćaš
  endTime: string;

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

  userFirstName?: string | null;
  userLastName?: string | null;
  trackName?: string | null;
  kartName?: string | null;
}

export interface CreateReservationCommand {
  userId: number;
  trackId: number;
  kartId: number;

  reservationDate: string; // "YYYY-MM-DD" ili ISO, kako backend očekuje
  startTime: string;       // "HH:mm" ili "HH:mm:ss"
  endTime: string;         // "HH:mm" ili "HH:mm:ss"
}

export interface UpdateReservationCommand {
  userId?: number | null;
  trackId?: number | null;
  kartId?: number | null;

  reservationDate?: string | null;
  startTime?: string | null;
  endTime?: string | null;
}
