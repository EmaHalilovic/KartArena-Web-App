import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, defer, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListReservationRequest,
  ListReservationResponse,
  GetReservationByIdQueryDto,
  CreateReservationCommand,
  CheckoutReservationsRequest,
  UpdateReservationCommand,
  MarkReservationCashPaidPayload,
  MarkReservationCashPaidCommand,
  ChangeReservationStatusCommand,
  ReservationStatus,
  ReservationEmployeeAssignmentDto,
  AvailableReservationEquipmentItemDto,
  AssignReservationResourcesPayload,
  AssignReservationResourcesCommand,
  GetReservationAvailabilityDto,
} from './reservation-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({ providedIn: 'root' })
export class ReservationApiService {
  private readonly apiUrl = `${environment.apiUrl}/api`;
  private readonly baseUrl = `${environment.apiUrl}/reservations`;
  private http = inject(HttpClient);

  list(request?: ListReservationRequest): Observable<ListReservationResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListReservationResponse>(this.baseUrl, { params });
  }

  getById(id: number): Observable<GetReservationByIdQueryDto> {
    return this.http.get<GetReservationByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreateReservationCommand): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload);
  }

  checkout(payload: CheckoutReservationsRequest): Observable<number[]> {
    return this.http.post<number[]>(`${this.apiUrl}/reservations/checkout`, payload);
  }

  getAvailability(date: string, duration: number): Observable<GetReservationAvailabilityDto> {
  return this.http.get<GetReservationAvailabilityDto>(
    `${this.apiUrl}/reservations/availability`,
    {
      params: {
        date,
        duration,
      },
    }
  );
}
  update(id: number, payload: UpdateReservationCommand): Observable<number> {
    return this.http.put<number>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  confirmReservation(id: number): Observable<void> {
    return defer(() =>
      throwError(() => new Error(`TODO: connect reservation confirm endpoint for reservation ${id}.`))
    );
  }

  markCashPaid(id: number, payload: MarkReservationCashPaidPayload): Observable<number> {
    const command: MarkReservationCashPaidCommand = {
      reservationId: id,
      transactionReference: payload.transactionReference ?? null,
      note: payload.note ?? null,
    };

    return this.http.put<number>(`${this.baseUrl}/${id}/pay-cash`, command);
  }

  changeStatus(
    id: number,
    status: ReservationStatus.Completed | ReservationStatus.Cancelled
  ): Observable<number> {
    const command: ChangeReservationStatusCommand = { reservationId: id, status };
    return this.http.put<number>(`${this.baseUrl}/${id}/status`, command);
  }

  getReservationAssignments(reservationId: number): Observable<ReservationEmployeeAssignmentDto[]> {
    return this.http.get<ReservationEmployeeAssignmentDto[]>(`${this.baseUrl}/${reservationId}/assignments`);
  }

  getAvailableEquipmentForReservation(
    reservationId: number,
    categoryId: number
  ): Observable<AvailableReservationEquipmentItemDto[]> {
    return this.http.get<AvailableReservationEquipmentItemDto[]>(
      `${this.baseUrl}/${reservationId}/available-equipment`,
      {
        params: {
          categoryId,
        },
      }
    );
  }

  saveReservationAssignments(id: number, payload: AssignReservationResourcesPayload): Observable<number> {
    const command: AssignReservationResourcesCommand = {
      reservationId: id,
      employeeId: payload.employeeId ?? null,
      employeeName: payload.employeeName?.trim() || null,
      equipmentItemIds: payload.equipmentItemIds ?? [],
      replaceExistingAssignments: payload.replaceExistingAssignments ?? true,
    };

    return this.http.post<number>(`${this.baseUrl}/${id}/assignments`, command);
  }

  removeReservationAssignment(assignmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/assignments/${assignmentId}`);
  }

  assignResources(id: number, payload: AssignReservationResourcesPayload): Observable<number> {
    return this.saveReservationAssignments(id, payload);
  }
}
