import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import {
  CreateEquipmentTypeCommand,
  EquipmentTypeDetailsDto,
} from '../../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../../api-services/equipment/equipment-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { EquipmentFormService } from '../services/equipment-form.service';

@Component({
  selector: 'app-equipment-add',
  standalone: false,
  templateUrl: './equipment-add.component.html',
  styleUrl: './equipment-add.component.scss',
  providers: [EquipmentFormService],
})
export class EquipmentAddComponent
  extends BaseFormComponent<EquipmentTypeDetailsDto>
  implements OnInit
{
  private api = inject(EquipmentApiService);
  private formService = inject(EquipmentFormService);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  ngOnInit(): void {
    this.initForm(false);
  }

  protected loadData(): void {}

  protected override initForm(isEdit: boolean): void {
    super.initForm(isEdit);
    this.form = this.formService.createEquipmentTypeForm();
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const command: CreateEquipmentTypeCommand = {
      name: this.form.value.name,
      category: this.form.value.category,
      size: this.form.value.size,
      price: this.form.value.price,
      description: this.form.value.description,
    };

    this.api.create(command).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Equipment type created successfully');
        this.router.navigate(['/admin/equipment']);
      },
      error: (err) => {
        this.stopLoading('Failed to create equipment type');
        console.error('Create equipment type error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/equipment']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
