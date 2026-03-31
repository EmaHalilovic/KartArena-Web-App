import { Component, inject, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { ToasterService } from '../../../../core/services/toaster.service';
import { ReservationApiService } from '../../../../api-services/reservations/reservation-api.service';
import { ReservationFormService } from '../services/reservation-form.service';

@Component({
  selector: 'app-reservation-edit',
  standalone: false,
  templateUrl: './reservation-edit.component.html',
  styleUrl: './reservation-edit.component.scss',
  providers: [ReservationFormService],
})
export class ReservationEditComponent implements OnInit {
  private api = inject(ReservationApiService);
  private formService = inject(ReservationFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  form!: FormGroup;
  isLoading = false;
  isSaving = false;
  id!: number;

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.form = this.formService.createReservationForm();
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.getById(this.id).subscribe({
      next: (dto) => {
        this.isLoading = false;
        this.form = this.formService.createReservationForm(dto);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Get reservation error:', err);
        this.toaster.error('Reservation not found');
        this.router.navigate(['/admin/reservation']);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/reservations']);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.api.update(this.id, this.form.value).subscribe({
      next: () => {
        this.isSaving = false;
        this.toaster.success('Reservation updated');
        this.router.navigate(['/admin/reservations']);
      },
      error: (err) => {
        this.isSaving = false;
        console.error('Update reservation error:', err);
        this.toaster.error('Update reservation failed');
      },
    });
  }

  err(name: string): string {
    return this.formService.getError(this.form, name);
  }
}
