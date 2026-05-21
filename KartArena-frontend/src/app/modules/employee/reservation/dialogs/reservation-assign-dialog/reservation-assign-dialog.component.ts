import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { EquipmentApiService } from '../../../../../api-services/equipment/equipment-api.service';
import {
  ListEquipmentQueryDto,
  ListEquipmentRequest,
} from '../../../../../api-services/equipment/equipment-api.models';
import { ReservationApiService } from '../../../../../api-services/reservations/reservation-api.service';
import {
  AssignReservationResourcesPayload,
  AvailableReservationEquipmentItemDto,
  ReservationEmployeeAssignmentDto,
} from '../../../../../api-services/reservations/reservation-api.models';

export interface ReservationAssignDialogData {
  reservationId: number;
  reservationCode: string;
  customerName: string;
}

interface SelectedAssignmentItem {
  assignmentId?: number | null;
  equipmentItemId: number;
  itemCode: string;
  equipmentTypeId?: number | null;
  equipmentTypeName?: string | null;
}

@Component({
  selector: 'app-reservation-assign-dialog',
  templateUrl: './reservation-assign-dialog.component.html',
  styleUrl: './reservation-assign-dialog.component.scss',
  standalone: false,
})
export class ReservationAssignDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly equipmentApi = inject(EquipmentApiService);
  private readonly reservationApi = inject(ReservationApiService);

  readonly form = this.fb.group({
    employeeName: ['', [Validators.required]],
    equipmentTypeId: [null as number | null],
    pendingEquipmentItemIds: [[] as number[]],
  });

  equipmentTypeOptions: ListEquipmentQueryDto[] = [];
  availableEquipmentOptions: AvailableReservationEquipmentItemDto[] = [];
  selectedAssignments: SelectedAssignmentItem[] = [];

  isLoadingInitial = false;
  isLoadingEquipmentTypes = false;
  isLoadingAssignments = false;
  isLoadingAvailableEquipment = false;
  loadError = '';

  constructor(
    private readonly ref: MatDialogRef<ReservationAssignDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public readonly data: ReservationAssignDialogData
  ) {}

  ngOnInit(): void {
    this.loadInitialData();
  }

  cancel(): void {
    this.ref.close();
  }

  onEquipmentTypeChange(equipmentTypeId: number | null): void {
    this.form.patchValue(
      {
        equipmentTypeId,
        pendingEquipmentItemIds: [],
      },
      { emitEvent: false }
    );

    if (!equipmentTypeId) {
      this.availableEquipmentOptions = [];
      return;
    }

    this.loadAvailableEquipment(equipmentTypeId);
  }

  addSelectedItems(): void {
    const selectedIds = this.form.value.pendingEquipmentItemIds ?? [];
    if (!selectedIds.length) {
      return;
    }

    const itemsToAdd = this.availableEquipmentOptions.filter((item) => selectedIds.includes(item.id));
    for (const item of itemsToAdd) {
      if (this.selectedAssignments.some((existing) => existing.equipmentItemId === item.id)) {
        continue;
      }

      this.selectedAssignments = [
        ...this.selectedAssignments,
        {
          assignmentId: null,
          equipmentItemId: item.id,
          itemCode: item.itemCode,
          equipmentTypeId: item.equipmentTypeId,
          equipmentTypeName: item.equipmentTypeName,
        },
      ];
    }

    this.form.patchValue({ pendingEquipmentItemIds: [] }, { emitEvent: false });
  }

  removeSelectedItem(itemId: number): void {
    this.selectedAssignments = this.selectedAssignments.filter((item) => item.equipmentItemId !== itemId);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: AssignReservationResourcesPayload = {
      employeeId: null,
      employeeName: this.form.value.employeeName?.trim() || null,
      equipmentItemIds: this.selectedAssignments.map((item) => item.equipmentItemId),
      replaceExistingAssignments: true,
    };

    this.ref.close(payload);
  }

  isAlreadySelected(itemId: number): boolean {
    return this.selectedAssignments.some((item) => item.equipmentItemId === itemId);
  }

  get canAddPendingItems(): boolean {
    return (this.form.value.pendingEquipmentItemIds?.length ?? 0) > 0;
  }

  get hasSelectedAssignments(): boolean {
    return this.selectedAssignments.length > 0;
  }

  trackByEquipmentType(_: number, item: ListEquipmentQueryDto): number {
    return item.id;
  }

  trackByAssignment(_: number, item: SelectedAssignmentItem): number {
    return item.equipmentItemId;
  }

  private loadInitialData(): void {
    this.isLoadingInitial = true;
    this.loadError = '';
    this.loadEquipmentTypes();
    this.loadAssignments();
  }

  private loadEquipmentTypes(): void {
    const request = new ListEquipmentRequest();
    request.onlyActive = true;
    request.paging.page = 1;
    request.paging.pageSize = 200;

    this.isLoadingEquipmentTypes = true;
    this.equipmentApi.list(request).subscribe({
      next: (response) => {
        this.equipmentTypeOptions = response.items;
        this.isLoadingEquipmentTypes = false;
        this.finishInitialLoadIfReady();
      },
      error: () => {
        this.equipmentTypeOptions = [];
        this.isLoadingEquipmentTypes = false;
        this.loadError = 'Failed to load equipment categories.';
        this.finishInitialLoadIfReady();
      },
    });
  }

  private loadAssignments(): void {
    this.isLoadingAssignments = true;
    this.reservationApi.getReservationAssignments(this.data.reservationId).subscribe({
      next: (assignments) => {
        this.applyExistingAssignments(assignments);
        this.isLoadingAssignments = false;
        this.finishInitialLoadIfReady();
      },
      error: () => {
        this.loadError = 'Failed to load reservation assignments.';
        this.isLoadingAssignments = false;
        this.finishInitialLoadIfReady();
      },
    });
  }

  private applyExistingAssignments(assignments: ReservationEmployeeAssignmentDto[]): void {
    const employeeNames = [...new Set(assignments.map((item) => item.employeeName?.trim()).filter(Boolean))];
    this.form.patchValue({
      employeeName: employeeNames.length === 1 ? employeeNames[0] : '',
    });

    this.selectedAssignments = assignments
      .filter((item) => item.equipmentItemId != null)
      .map((item) => ({
        assignmentId: item.id,
        equipmentItemId: item.equipmentItemId!,
        itemCode: item.equipmentItemName?.trim() || `#${item.equipmentItemId}`,
        equipmentTypeId: item.equipmentTypeId ?? null,
        equipmentTypeName: item.equipmentCategoryName ?? null,
      }));
  }

  private loadAvailableEquipment(equipmentTypeId: number): void {
    this.isLoadingAvailableEquipment = true;
    this.availableEquipmentOptions = [];

    this.reservationApi.getAvailableEquipmentForReservation(this.data.reservationId, equipmentTypeId).subscribe({
      next: (items) => {
        this.availableEquipmentOptions = items;
        this.isLoadingAvailableEquipment = false;
      },
      error: () => {
        this.availableEquipmentOptions = [];
        this.isLoadingAvailableEquipment = false;
        this.loadError = 'Failed to load available equipment for the selected category.';
      },
    });
  }

  private finishInitialLoadIfReady(): void {
    if (this.isLoadingEquipmentTypes || this.isLoadingAssignments) {
      return;
    }

    this.isLoadingInitial = false;
  }
}
