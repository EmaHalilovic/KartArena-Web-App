import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  AddEquipmentItemCommand,
  BulkAddEquipmentItemsCommand,
  EquipmentCategory,
  EquipmentItemStatus,
  EquipmentStockStatus,
  GetEquipmentByIdQueryDto,
  GetEquipmentByIdQueryDtoItem,
} from '../../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../../api-services/equipment/equipment-api.service';
import { BaseComponent } from '../../../../core/components/base-classes/base-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import {
  buildEquipmentSummaryCards,
  getEquipmentCategoryLabel,
  getEquipmentStockLabel,
  getEquipmentStockTone,
  getItemStatusLabel,
  getItemStatusTone,
} from '../equipment-ui.helpers';
import { EquipmentAddItemDialogComponent } from '../dialogs/equipment-add-item-dialog/equipment-add-item-dialog.component';
import { EquipmentBulkAddItemsDialogComponent } from '../dialogs/equipment-bulk-add-items-dialog/equipment-bulk-add-items-dialog.component';
import { EquipmentEditItemDialogComponent } from '../dialogs/equipment-edit-item-dialog/equipment-edit-item-dialog.component';
import { EquipmentItemApiService } from '../../../../api-services/equipment items/equipmentItem-api.service';
import { CreateEquipmentItemCommand, UpdateEquipmentItemCommand } from '../../../../api-services/equipment items/equipmentItem-api.models';
import { ConfirmDeleteDialogComponent } from '../dialogs/confirm-delete/confirm-delete-dialog.component';

@Component({
  selector: 'app-equipment-detail',
  standalone: false,
  templateUrl: './equipment-detail.component.html',
  styleUrl: './equipment-detail.component.scss',
})
export class EquipmentDetailComponent extends BaseComponent implements OnInit {
  protected readonly itemStatus = EquipmentItemStatus;
  protected readonly itemStatusOptions = Object.values(EquipmentItemStatus);

  private api = inject(EquipmentApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);
  private ItemsApi=inject(EquipmentItemApiService);


  equipmentId!: number;
  equipment?: GetEquipmentByIdQueryDto;
  itemSearch = '';
  selectedItemStatus: EquipmentItemStatus | 'all' = 'all';
  summaryCards = buildEquipmentSummaryCards({
    id: 0,
    name: '',
    category: EquipmentCategory.Other,
    size: '',
    price: 0,
    totalItems: 0,
    availableItems: 0,
    lostItems: 0,
    maintenanceItems: 0,
    inUseItems: 0,
    stockStatus: EquipmentStockStatus.OutOfStock,
    isActive: false,
    items: [],
  });
  displayedColumns = ['itemCode', 'status', 'purchaseDate', 'lastInspectedAt', 'notes', 'actions'];

  ngOnInit(): void {
    this.equipmentId = +this.route.snapshot.params['id'];
    this.loadEquipment();
  }

  onBack(): void {
    this.router.navigate(['/admin/equipment']);
  }

  onEditType(): void {
    this.router.navigate(['/admin/equipment', this.equipmentId, 'edit']);
  }

  onToggleActive(): void {
    if (!this.equipment) {
      return;
    }

    const request$ = this.equipment.isActive
      ? this.api.disable(this.equipment.id)
      : this.api.enable(this.equipment.id);

    request$.subscribe({
      next: () => {
        this.toaster.success(`Equipment type ${this.equipment?.isActive ? 'deactivated' : 'activated'} successfully`);
        this.loadEquipment();
      },
      error: (err) => {
        console.error('Toggle equipment type error:', err);
        this.toaster.error('Failed to update equipment type status');
      },
    });
  }

  onAddItem(): void {
    const ref = this.dialog.open(EquipmentAddItemDialogComponent, {
      width: '620px',
      maxWidth: '96vw',
      data: { equipmentName: this.equipment?.name ?? '' },
    });

    ref.afterClosed().subscribe((payload?: AddEquipmentItemCommand) => {
      if (!payload || !this.equipment) {
        return;
      }

      const command: CreateEquipmentItemCommand = {
        itemCode: payload.itemCode,
        equipmentTypeId: this.equipment.id,
        status: payload.status as CreateEquipmentItemCommand['status'],
        purchaseDate: payload.purchaseDate,
        lastInspectedAt: payload.lastInspectedAt,
        notes: payload.notes,
      };

      this.ItemsApi.create(command).subscribe({
        next: () => {
          this.toaster.success('Equipment item added successfully');
          this.loadEquipment();
        },
        error: (err) => {
          console.error('Add equipment item error:', err);
          this.toaster.error('Failed to add equipment item');
        },
      });
    });
  }

  onBulkAddItems(): void {
    const ref = this.dialog.open(EquipmentBulkAddItemsDialogComponent, {
      width: '680px',
      maxWidth: '96vw',
      data: { equipmentName: this.equipment?.name ?? '', equipmentSize: this.equipment?.size ?? '' },
    });

    ref.afterClosed().subscribe((payload?: BulkAddEquipmentItemsCommand) => {
      if (!payload || !this.equipment) {
        return;
      }

      const numberPadding = payload.numberPadding ?? 3;
      const createRequests = Array.from({ length: payload.count }, (_, index) => {
        const itemNumber = String(payload.startNumber + index).padStart(numberPadding, '0');
        const command: CreateEquipmentItemCommand = {
          itemCode: `${payload.prefix}${itemNumber}`,
          equipmentTypeId: this.equipment!.id,
          status: payload.status as CreateEquipmentItemCommand['status'],
          purchaseDate: payload.purchaseDate,
          lastInspectedAt: payload.lastInspectedAt,
          notes: payload.notes,
        };

        return this.ItemsApi.create(command);
      });

      forkJoin(createRequests).subscribe({
        next: () => {
          this.toaster.success('Equipment items added successfully');
          this.loadEquipment();
        },
        error: (err) => {
          console.error('Bulk add equipment items error:', err);
          this.toaster.error('Failed to bulk add equipment items');
        },
      });
    });
  }

  onExportPdf(): void {
    // if (!this.equipment) {
    //   return;
    // }

    // this.api.exportDetailsPdf(this.equipment.id).subscribe({
    //   next: (blob) => {
    //     this.downloadBlob(blob, `${this.equipment?.name ?? 'equipment-type'}.pdf`);
    //     this.toaster.success('Equipment detail PDF export started');
    //   },
    //   error: (err) => {
    //     console.error('Export equipment detail PDF error:', err);
    //     this.toaster.error('Failed to export equipment type PDF');
    //   },
    // });
  }

  getStockTone(): string {
    return this.equipment ? getEquipmentStockTone(this.equipment.stockStatus) : 'neutral';
  }

  getStockLabel(): string {
    return this.equipment ? getEquipmentStockLabel(this.equipment.stockStatus) : 'Unknown';
  }

  getCategoryLabel(): string {
    return this.equipment ? getEquipmentCategoryLabel(this.equipment.category) : 'Other';
  }

  getItemTone(status: EquipmentItemStatus): string {
    return getItemStatusTone(status);
  }

  getItemLabel(status: EquipmentItemStatus): string {
    return getItemStatusLabel(status);
  }

  get filteredItems(): GetEquipmentByIdQueryDtoItem[] {
    const items = this.equipment?.items ?? [];
    const search = this.itemSearch.trim().toLowerCase();

    return items.filter((item) => {
      const matchesSearch =
        !search ||
        item.itemCode.toLowerCase().includes(search) ;

      const matchesStatus =
        this.selectedItemStatus === 'all' || item.status === this.selectedItemStatus;

      return matchesSearch && matchesStatus;
    });
  }

  onSetItemStatus(item: GetEquipmentByIdQueryDtoItem, status: EquipmentItemStatus): void {
    if (item.status === status || !this.equipment) {
      return;
    }

    this.ItemsApi.update(item.id, {
      itemCode: item.itemCode,
      equipmentTypeId: this.equipment.id,
      status,
      purchaseDate: item.purchaseDate,
      lastInspectedAt: item.lastInspectedAt,
      notes: item.notes,
    }).subscribe({
      next: () => {
        this.toaster.success(`Equipment item marked as ${this.getItemLabel(status).toLowerCase()}`);
        this.loadEquipment();
      },
      error: (err) => {
        console.error('Update equipment item status error:', err);
        this.toaster.error('Failed to update equipment item status');
      },
    });
  }

  onEditItem(item: GetEquipmentByIdQueryDtoItem): void {
    const ref = this.dialog.open(EquipmentEditItemDialogComponent, {
      width: '620px',
      maxWidth: '96vw',
      data: {
        equipmentName: this.equipment?.name ?? '',
        item,
      },
    });

    ref.afterClosed().subscribe((payload?: UpdateEquipmentItemCommand) => {
      if (!payload || !this.equipment) {
        return;
      }

      this.ItemsApi.update(item.id, {
        ...payload,
        equipmentTypeId: this.equipment.id,
      }).subscribe({
        next: () => {
          this.toaster.success('Equipment item updated successfully');
          this.loadEquipment();
        },
        error: (err) => {
          console.error('Update equipment item error:', err);
          this.toaster.error('Failed to update equipment item');
        },
      });
    });
  }

  onDeleteItem(item: GetEquipmentByIdQueryDtoItem): void {
    const ref = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        titleKey: 'Delete equipment item',
        messageKey: 'This will permanently remove the selected equipment item.',
        name: item.itemCode,
        confirmKey: 'Delete',
        cancelKey: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }

      this.ItemsApi.delete(item.id).subscribe({
        next: () => {
          this.toaster.success('Equipment item deleted successfully');
          this.loadEquipment();
        },
        error: (err) => {
          console.error('Delete equipment item error:', err);
          this.toaster.error('Failed to delete equipment item');
        },
      });
    });
  }

  private loadEquipment(): void {
    this.startLoading();

    this.api.getById(this.equipmentId).subscribe({
      next: (equipment) => {
        this.equipment = equipment;
        this.summaryCards = buildEquipmentSummaryCards(equipment);
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load equipment type');
        console.error('Load equipment type detail error:', err);
        this.toaster.error('Equipment type not found');
      },
    });
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
