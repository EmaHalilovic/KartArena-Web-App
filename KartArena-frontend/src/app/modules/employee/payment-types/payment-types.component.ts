import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

import {
  ListPaymentTypesQueryDto,
  ListPaymentTypesRequest,
} from '../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../api-services/payment-types/payment-types-api.service';
import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
import { PageResult } from '../../../core/models/paging/page-result';
import { ToasterService } from '../../../core/services/toaster.service';
import { ConfirmDeletePaymentTypeDialogComponent } from './dialogs/confirm-delete/confirm-delete-dialog.component';

type PaymentMethodFilter = 'all' | 'online' | 'desk';

@Component({
  selector: 'app-payment-types',
  standalone: false,
  templateUrl: './payment-types.component.html',
  styleUrl: './payment-types.component.scss',
})
export class PaymentTypesComponent
  extends BaseListPagedComponent<ListPaymentTypesQueryDto, ListPaymentTypesRequest>
  implements OnInit
{
  private api = inject(PaymentTypesApiService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);

  paymentMethodFilter: PaymentMethodFilter = 'all';

  displayedColumns: string[] = [
    'name',
    'code',
    'allowedOnline',
    'allowedAtDesk',
    'description',
    'isEnabled',
    'actions',
  ];

  constructor() {
    super();
    this.request = new ListPaymentTypesRequest();
    this.request.paging = this.request.paging ?? { page: 1, pageSize: 10 };
    this.request.onlyEnabled = true;
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();
    this.request.search = this.request.search?.trim() || null;

    this.api.list(this.buildApiRequest()).subscribe({
      next: (response) => {
        this.handlePageResult(this.applyClientFilters(response));
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load payment types error:', err);
        this.stopLoading('Failed to load payment types');
      },
    });
  }

 

  onEdit(item: ListPaymentTypesQueryDto): void {
    this.router.navigate(['/employee/payment-types', item.id, 'edit']);
  }

  onDelete(item: ListPaymentTypesQueryDto): void {
    const ref = this.dialog.open(ConfirmDeletePaymentTypeDialogComponent, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        titleKey: 'Delete payment type',
        messageKey: 'This will permanently remove the payment type.',
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

  onToggleStatus(item: ListPaymentTypesQueryDto): void {
    const request$ = item.isEnabled ? this.api.disable(item.id) : this.api.enable(item.id);

    request$.subscribe({
      next: () => {
        this.toaster.success(`Payment type ${item.isEnabled ? 'deactivated' : 'activated'} successfully`);
        this.loadPagedData();
      },
      error: (err) => {
        console.error('Toggle payment type status error:', err);
        this.toaster.error('Failed to update payment type status');
      },
    });
  }

  onSearch(value: string): void {
    this.request.paging.page = 1;
    this.request.search = value?.trim() || null;
    this.loadPagedData();
  }

  onToggleEnabledFilter(checked: boolean): void {
    this.request.onlyEnabled = checked;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onPaymentMethodFilterChange(value: PaymentMethodFilter): void {
    this.paymentMethodFilter = value;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  formatDescription(item: ListPaymentTypesQueryDto): string {
    return (item.description ?? '').trim() || '-';
  }

  private buildApiRequest(): ListPaymentTypesRequest {
    const request = new ListPaymentTypesRequest();
    request.search = this.request.search?.trim() || null;
    request.onlyEnabled = this.request.onlyEnabled ?? null;
    request.paging.page = 1;
    request.paging.pageSize = 1000;
    return request;
  }

  private applyClientFilters(response: PageResult<ListPaymentTypesQueryDto>): PageResult<ListPaymentTypesQueryDto> {
    const filteredItems = response.items.filter((item) => this.matchesPaymentMethodFilter(item));
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
      pageSize,
      totalItems,
      totalPages,
    };
  }

  private matchesPaymentMethodFilter(item: ListPaymentTypesQueryDto): boolean {
    if (this.paymentMethodFilter === 'online') {
      return item.allowedOnline;
    }

    if (this.paymentMethodFilter === 'desk') {
      return item.allowedAtDesk;
    }

    return true;
  }

  private performDelete(item: ListPaymentTypesQueryDto): void {
    this.startLoading();

    this.api.delete(item.id).subscribe({
      next: () => {
        this.toaster.success('Payment type deleted successfully');
        this.loadPagedData();
      },
      error: (err) => {
        console.error('Delete payment type error:', err);
        this.stopLoading('Failed to delete payment type');
      },
    });
  }
}

