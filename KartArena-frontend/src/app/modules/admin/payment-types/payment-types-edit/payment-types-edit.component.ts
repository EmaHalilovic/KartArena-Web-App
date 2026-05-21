import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
  GetPaymentTypeByIdQueryDto,
  UpdatePaymentTypeCommand,
} from '../../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../../api-services/payment-types/payment-types-api.service';
import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { PaymentTypesFormService } from '../services/payment-types-form.service';

@Component({
  selector: 'app-payment-types-edit',
  standalone: false,
  templateUrl: './payment-types-edit.component.html',
  styleUrl: './payment-types-edit.component.scss',
  providers: [PaymentTypesFormService],
})
export class PaymentTypesEditComponent
  extends BaseFormComponent<GetPaymentTypeByIdQueryDto>
  implements OnInit
{
  private api = inject(PaymentTypesApiService);
  private formService = inject(PaymentTypesFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  paymentTypeId!: number;

  ngOnInit(): void {
    this.paymentTypeId = +this.route.snapshot.params['id'];
    this.initForm(true);
  }

  protected override initForm(isEdit: boolean): void {
    this.form = this.formService.createPaymentTypeForm();
    super.initForm(isEdit);
  }

  protected loadData(): void {
    this.startLoading();

    this.api.getById(this.paymentTypeId).subscribe({
      next: (paymentType) => {
        this.model = paymentType;
        this.form.patchValue({
          name: paymentType.name ?? '',
          code: paymentType.code ?? '',
          allowedOnline: paymentType.allowedOnline ?? false,
          allowedAtDesk: paymentType.allowedAtDesk ?? false,
          description: paymentType.description ?? '',
        });
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load payment type');
        this.toaster.error('Payment type not found');
        console.error('Load payment type error:', err);
        this.router.navigate(['/admin/payment-types']);
      },
    });
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const command: UpdatePaymentTypeCommand = {
      name: this.form.value.name,
      code: this.form.value.code,
      allowedOnline: this.form.value.allowedOnline,
      allowedAtDesk: this.form.value.allowedAtDesk,
      description: this.form.value.description,
    };

    this.api.update(this.paymentTypeId, command).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Payment type updated successfully');
        this.router.navigate(['/admin/payment-types']);
      },
      error: (err) => {
        this.stopLoading('Failed to update payment type');
        console.error('Update payment type error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/payment-types']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
