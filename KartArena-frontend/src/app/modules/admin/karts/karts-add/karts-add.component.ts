import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormGroup } from '@angular/forms';

import { KartsApiService } from '../../../../api-services/karts/karts-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { KartsFormService } from '../services/karts-form.service';

@Component({
  selector: 'app-karts-add',
  standalone: false,
  templateUrl: './karts-add.component.html',
  styleUrls: ['./karts-add.component.scss'],
  providers: [KartsFormService],
})
export class KartsAddComponent {
  private api = inject(KartsApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private formService = inject(KartsFormService);

  form: FormGroup = this.formService.createKartForm();
  loading = false;

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.api.create(this.form.value).subscribe({
      next: () => {
        this.toaster.success('Kart created successfully');
        this.router.navigate(['/admin/karts']);
      },
      error: (err: any) => {
        console.error(err);
        this.toaster.error('Failed to create kart');
      },
      complete: () => (this.loading = false),
    });
  }

  cancel(): void {
    this.router.navigate(['/admin/karts']);
  }

  getError(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
