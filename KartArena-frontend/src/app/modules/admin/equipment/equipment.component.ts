import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

import {
  EquipmentCategory,
  EquipmentStockStatus,
  ListEquipmentQueryDto,
  ListEquipmentRequest,
} from '../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../api-services/equipment/equipment-api.service';
import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
import { PageResult } from '../../../core/models/paging/page-result';
import { ToasterService } from '../../../core/services/toaster.service';
import { ConfirmDeleteDialogComponent } from './dialogs/confirm-delete/confirm-delete-dialog.component';
import {
  getEquipmentCategoryLabel,
  getEquipmentCategories,
  getEquipmentStockLabel,
  getEquipmentStockTone,
} from './equipment-ui.helpers';

@Component({
  selector: 'app-equipment',
  standalone: false,
  templateUrl: './equipment.component.html',
  styleUrl: './equipment.component.scss',
})
export class EquipmentComponent
  extends BaseListPagedComponent<ListEquipmentQueryDto, ListEquipmentRequest>
  implements OnInit
{
  private api = inject(EquipmentApiService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);

  displayedColumns: string[] = [
    'name',
    'category',
    'size',
    'price',
    'totalItems',
    'availableItems',
    'stockStatus',
    'isActive',
    'actions',
  ];

  stockStatusOptions = Object.values(EquipmentStockStatus);
  categoryOptions = getEquipmentCategories();
  onlyActive = true;

  constructor() {
    super();
    this.request = new ListEquipmentRequest();
    this.request.onlyActive = this.onlyActive;
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.buildApiRequest()).subscribe({
      next: (response) => {
        console.log('paged response', response);
        this.handlePageResult(this.applyClientFilters(response));
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load equipment types');
        console.error('Load equipment types error:', err);
      },
    });
  }

  onCreate(): void {
    this.router.navigate(['/admin/equipment/add']);
  }

  onView(item: ListEquipmentQueryDto): void {
    this.router.navigate(['/admin/equipment', item.id]);
  }

  onEdit(item: ListEquipmentQueryDto): void {
    this.router.navigate(['/admin/equipment', item.id, 'edit']);
  }

  onDelete(item: ListEquipmentQueryDto): void {
    const ref = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        titleKey: 'Delete equipment type',
        messageKey: 'This will remove the equipment type and all associated items.',
        name: item.name,
        confirmKey: 'Delete',
        cancelKey: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.performDelete(item);
      }
    });
  }

  onToggleActive(item: ListEquipmentQueryDto): void {
    const request$ = item.isActive
      ? this.api.disable(item.id)
      : this.api.enable(item.id);

    request$.subscribe({
      next: () => {
        this.toaster.success(`Equipment type ${item.isActive ? 'deactivated' : 'activated'} successfully`);
        this.loadPagedData();
      },
      error: (err) => {
        console.error('Toggle equipment type error:', err);
        this.toaster.error('Failed to update equipment type status');
      },
    });
  }

  onSearch(search: string): void {
    this.request.paging.page = 1;
    this.request.search = search;
    this.loadPagedData();
  }

  onToggleActiveFilter(checked: boolean): void {
    this.onlyActive = checked;
    this.request.onlyActive = checked;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onCategoryChange(category: EquipmentCategory | null): void {
    this.request.category = category;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onStockStatusChange(status: EquipmentStockStatus | null): void {
    this.request.stockStatus = status;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onExportPdf(): void {
    // this.api.exportListPdf(this.request).subscribe({
    //   next: (blob) => {
    //     this.downloadBlob(blob, 'equipment-types.pdf');
    //     this.toaster.success('Equipment PDF export started');
    //   },
    //   error: (err) => {
    //     console.error('Export equipment PDF error:', err);
    //     this.toaster.error('Failed to export equipment PDF');
    //   },
    // });
   }

  getStockTone(item: ListEquipmentQueryDto): string {
    return getEquipmentStockTone(item.stockStatus);
  }

  getStockLabel(item: ListEquipmentQueryDto): string {
    return getEquipmentStockLabel(item.stockStatus);
  }

  getCategoryLabel(item: ListEquipmentQueryDto): string {
    return getEquipmentCategoryLabel(item.category);
  }

  private buildApiRequest(): ListEquipmentRequest {
    const request = new ListEquipmentRequest();
    request.search = this.request.search?.trim() || null;
    request.onlyActive = this.request.onlyActive ?? this.onlyActive;
    request.category = null;
    request.stockStatus = null;
    request.paging.page = 1;
    request.paging.pageSize = 1000;
    return request;
  }

  private applyClientFilters(response: PageResult<ListEquipmentQueryDto>): PageResult<ListEquipmentQueryDto> {
    const filteredItems = response.items.filter((item) => this.matchesFilters(item));
    const pageSize = this.request.paging.pageSize;
    const currentPage = this.request.paging.page;
    const totalItems = filteredItems.length;
    const totalPages = totalItems === 0 ? 0 : Math.ceil(totalItems / pageSize);
    const safePage = totalPages === 0 ? 1 : Math.min(currentPage, totalPages);
    const startIndex = (safePage - 1) * pageSize;

    this.request.paging.page = safePage;

    return {
      ...response,
      items: filteredItems.slice(startIndex, startIndex + pageSize),
      currentPage: safePage,
      totalItems,
      totalPages,
    };
  }

  private matchesFilters(item: ListEquipmentQueryDto): boolean {
    const search = (this.request.search ?? '').trim().toLowerCase();
    const categoryLabel = getEquipmentCategoryLabel(item.category).toLowerCase();

    if (search) {
      const matchesSearch =
        item.name.toLowerCase().includes(search) ||
        item.size.toLowerCase().includes(search) ||
        categoryLabel.includes(search);

      if (!matchesSearch) {
        return false;
      }
    }

    const onlyActive = this.request.onlyActive ?? this.onlyActive;

    if (item.isActive !== onlyActive) {
      return false;
    }

    if (this.request.category && item.category !== this.request.category) {
      return false;
    }

    if (this.request.stockStatus && item.stockStatus !== this.request.stockStatus) {
      return false;
    }

    return true;
  }

  private performDelete(item: ListEquipmentQueryDto): void {
    this.startLoading();

    this.api.delete(item.id).subscribe({
      next: () => {
        this.toaster.success('Equipment type deleted successfully');
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading('Failed to delete equipment type');
        console.error('Delete equipment type error:', err);
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
