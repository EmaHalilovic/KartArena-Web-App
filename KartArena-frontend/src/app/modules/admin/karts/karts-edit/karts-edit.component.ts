import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';

import { KartsApiService } from '../../../../api-services/karts/karts-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { KartsFormService } from '../services/karts-form.service';

@Component({
  selector: 'app-karts-edit',
  standalone: false,
  templateUrl: './karts-edit.component.html',
  styleUrls: ['./karts-edit.component.scss'],
  providers: [KartsFormService],
})
export class KartsEditComponent implements OnInit {
  private api = inject(KartsApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private formService = inject(KartsFormService);

  id!: number;
  form!: FormGroup;
  loading = false;

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.loading = true;

    this.api.getById(this.id).subscribe({
      next: (kart) => {
        this.form = this.formService.createKartForm(kart);
      },
      error: (err: any) => {
        console.error(err);
        this.toaster.error('Failed to load kart');
        this.router.navigate(['/admin/karts']);
      },
      complete: () => (this.loading = false),
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.api.update(this.id, this.form.value).subscribe({
      next: () => {
        this.toaster.success('Kart updated successfully');
        this.router.navigate(['/admin/karts']);
      },
      error: (err: any) => {
        console.error(err);
        this.toaster.error('Failed to update kart');
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
