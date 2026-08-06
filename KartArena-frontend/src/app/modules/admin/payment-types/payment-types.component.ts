import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

import {
  ListPaymentTypesQueryDto,
  ListPaymentTypesRequest,
} from '../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../api-services/payment-types/payment-types-api.service';
import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
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
    this.request.paging.pageSize = 10;
    this.request.onlyEnabled = true;
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();
    this.api.list(this.buildApiRequest()).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load payment types error:', err);
        this.stopLoading('Failed to load payment types');
      },
    });
  }

  onCreate(): void {
    this.router.navigate(['/admin/payment-types/add']);
  }

  onEdit(item: ListPaymentTypesQueryDto): void {
    this.router.navigate(['/admin/payment-types', item.id, 'edit']);
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

  onNameFilterChange(value: string): void {
    this.request.paging.page = 1;
    this.request.name = value?.trim() || null;
    this.loadPagedData();
  }

  onCodeFilterChange(value: string): void {
    this.request.paging.page = 1;
    this.request.code = value?.trim() || null;
    this.loadPagedData();
  }

  onToggleEnabledFilter(checked: boolean): void {
    this.request.onlyEnabled = checked;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onPaymentMethodFilterChange(value: PaymentMethodFilter): void {
    this.paymentMethodFilter = value;
    this.request.paymentMethod = value === 'all' ? null : value;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onAllowedOnlineFilterChange(value: boolean | null): void {
    this.request.allowedOnline = value;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  formatDescription(item: ListPaymentTypesQueryDto): string {
    return (item.description ?? '').trim() || '-';
  }

  private buildApiRequest(): ListPaymentTypesRequest {
    const request = new ListPaymentTypesRequest();
    request.name = this.request.name?.trim() || null;
    request.code = this.request.code?.trim() || null;
    request.paymentMethod = this.request.paymentMethod ?? null;
    request.allowedOnline = this.request.allowedOnline ?? null;
    request.onlyEnabled = this.request.onlyEnabled ?? null;
    request.paging.page = this.request.paging.page;
    request.paging.pageSize = this.request.paging.pageSize;
    return request;
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
