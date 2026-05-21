import { Component, inject, OnInit } from '@angular/core';
import {
  ListPaymentsQueryDto,
  ListPaymentsRequest,
  PaymentStatus,
} from '../../../api-services/payments/payments-api.models';
import { PaymentsApiService } from '../../../api-services/payments/payments-api.service';
import {
  ListPaymentTypesQueryDto,
  ListPaymentTypesRequest,
} from '../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../api-services/payment-types/payment-types-api.service';
import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';

@Component({
  selector: 'app-payments',
  standalone: false,
  templateUrl: './payments.component.html',
  styleUrl: './payments.component.scss',
})
export class PaymentsComponent
  extends BaseListPagedComponent<ListPaymentsQueryDto, ListPaymentsRequest>
  implements OnInit
{

  private api = inject(PaymentsApiService);
  private paymentTypesApi = inject(PaymentTypesApiService);

  displayedColumns: string[] = [
    'id',
    'customerName',
    'reservationDate',
    'amount',
    'paymentDate',
    'paymentTypeName',
    'status',
  ];
  readonly paymentStatusOptions = [
    { value: PaymentStatus.Pending, label: 'Pending' },
    { value: PaymentStatus.Paid, label: 'Paid' },
    { value: PaymentStatus.Failed, label: 'Failed' },
    { value: PaymentStatus.Refunded, label: 'Refunded' },
  ];
  paymentTypeOptions: ListPaymentTypesQueryDto[] = [];

  constructor() {
    super();
    this.request = new ListPaymentsRequest();
    this.request.paging = this.request.paging ?? { page: 1, pageSize: 10 };
  }

  ngOnInit(): void {
    this.loadPaymentTypeOptions();
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
        this.handlePageResult(response);
        this.stopLoading();
      },
      error: (err) => {
        console.error('Load payments error:', err);
        this.stopLoading('Failed to load payments');
      },
    });
  }

  onSearch(value:string): void {
    this.request.search = value?.trim() || null;
    this.loadPagedData();
  }

  onStatusChange(value: PaymentStatus | null): void {
    this.request.status = value;
    this.loadPagedData();
  }
  onSearchPaymentType(value: number | null): void {
    this.request.paymentTypeId = value;
    this.loadPagedData();
  }

  formatCustomerName(r: ListPaymentsQueryDto): string {
    return (r.customerName ?? '').trim() || '-';
  }

  formatReservationDate(r: ListPaymentsQueryDto): string {
    return (r.reservationDate ?? '').trim() || '-';
  }

  formatPaymentType(r: ListPaymentsQueryDto): string {
    return (r.paymentTypeName ?? '').trim() || '-';
  }

  getStatusTone(status: PaymentStatus): 'good' | 'fair' | 'low' | 'neutral' {
    switch (status) {
      case PaymentStatus.Paid:
        return 'good';
      case PaymentStatus.Pending:
        return 'fair';
      case PaymentStatus.Failed:
        return 'low';
      case PaymentStatus.Refunded:
        return 'neutral';
      default:
        return 'neutral';
    }
  }

  getStatusLabel(status: PaymentStatus): string {
    return this.paymentStatusOptions.find((option) => option.value === status)?.label ?? 'Unknown';
  }

  private loadPaymentTypeOptions(): void {
    const request = new ListPaymentTypesRequest();
    request.onlyEnabled = true;
    request.paging.page = 1;
    request.paging.pageSize = 1000;

    this.paymentTypesApi.list(request).subscribe({
      next: (response) => {
        this.paymentTypeOptions = response.items;
      },
      error: (err) => {
        console.error('Load payment types error:', err);
      },
    });
  }

  private normalizeNumberFilter(value?: number | null): number | null {
    if (value == null || Number.isNaN(value)) {
      return null;
    }

    return value;
  }
}
