import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import {
  CreatePaymentTypeCommand,
  GetPaymentTypeByIdQueryDto,
} from '../../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../../api-services/payment-types/payment-types-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { PaymentTypesFormService } from '../services/payment-types-form.service';

@Component({
  selector: 'app-payment-types-add',
  standalone: false,
  templateUrl: './payment-types-add.component.html',
  styleUrl: './payment-types-add.component.scss',
  providers: [PaymentTypesFormService],
})
export class PaymentTypesAddComponent
  extends BaseFormComponent<GetPaymentTypeByIdQueryDto>
  implements OnInit
{
  private api = inject(PaymentTypesApiService);
  private formService = inject(PaymentTypesFormService);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  ngOnInit(): void {
    this.initForm(false);
  }

  protected loadData(): void {}

  protected override initForm(isEdit: boolean): void {
    super.initForm(isEdit);
    this.form = this.formService.createPaymentTypeForm();
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const command: CreatePaymentTypeCommand = {
      name: this.form.value.name,
      code: this.form.value.code,
      allowedOnline: this.form.value.allowedOnline,
      allowedAtDesk: this.form.value.allowedAtDesk,
      description: this.form.value.description,
    };

    this.api.create(command).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Payment type created successfully');
        this.router.navigate(['/admin/payment-types']);
      },
      error: (err) => {
        this.stopLoading('Failed to create payment type');
        console.error('Create payment type error:', err);
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
