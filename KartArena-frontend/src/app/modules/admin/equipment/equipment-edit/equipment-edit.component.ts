import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  EquipmentTypeDetailsDto,
  UpdateEquipmentTypeCommand,
} from '../../../../api-services/equipment/equipment-api.models';
import { EquipmentApiService } from '../../../../api-services/equipment/equipment-api.service';
import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { EquipmentFormService } from '../services/equipment-form.service';

@Component({
  selector: 'app-equipment-edit',
  standalone: false,
  templateUrl: './equipment-edit.component.html',
  styleUrl: './equipment-edit.component.scss',
  providers: [EquipmentFormService],
})
export class EquipmentEditComponent
  extends BaseFormComponent<EquipmentTypeDetailsDto>
  implements OnInit
{
  private api = inject(EquipmentApiService);
  private formService = inject(EquipmentFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  equipmentId!: number;

  ngOnInit(): void {
    this.equipmentId = +this.route.snapshot.params['id'];
    this.initForm(true);
  }

  protected override initForm(isEdit: boolean): void {
    this.form = this.formService.createEquipmentTypeForm();
    super.initForm(isEdit);
  }

  protected loadData(): void {
    this.startLoading();

    this.api.getById(this.equipmentId).subscribe({
      next: (equipment) => {
        this.model = equipment;
        this.form.patchValue({
          name: equipment.name ?? '',
          category: this.formService.toCategoryOptionValue(equipment.category),
          size: equipment.size ?? '',
          description: equipment.description ?? '',
          price: equipment.price ?? 0,
        });
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load equipment type');
        this.toaster.error('Equipment type not found');
        console.error('Load equipment type error:', err);
        this.router.navigate(['/admin/equipment']);
      },
    });
  }

  protected save(): void {
    if (this.form.invalid || this.isLoading) {
      return;
    }

    this.startLoading();

    const formValue = this.form.getRawValue();
   

    
    this.api.update(this.equipmentId, formValue).subscribe({
      next: () => {
        this.stopLoading();
        this.toaster.success('Equipment type updated successfully');
        this.router.navigate(['/admin/equipment', this.equipmentId]);
      },
      error: (err) => {
        this.stopLoading('Failed to update equipment type');
        console.error('Update equipment type error:', err);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/equipment', this.equipmentId]);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
