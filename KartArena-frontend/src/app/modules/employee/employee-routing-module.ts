import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { EmployeeLayoutComponent } from './employee-layout/employee-layout.component';
import { EquipmentComponent } from './equipment/equipment.component';
import { EquipmentAddComponent } from './equipment/equipment-add/equipment-add.component';
import { EquipmentEditComponent } from './equipment/equipment-edit/equipment-edit.component';
import { EquipmentDetailComponent } from './equipment/equipment-detail/equipment-detail.component';
import { ReservationComponent } from './reservation/reservation.component';
import { ReservationAddComponent } from './reservation/reservation-add/reservation-add.component';
import { ReservationDetailsComponent } from './reservation/reservation-details/reservation-details.component';
import { PaymentsComponent } from './payments/payments.component';
import { PaymentTypesComponent } from './payment-types/payment-types.component';
import { PaymentTypesEditComponent } from './payment-types/payment-types-edit/payment-types-edit.component';
import { EmployeeSettingsComponent } from './employee-settings/employee-settings.component';
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';

const routes: Routes = [
  {
    path: '',
    component: EmployeeLayoutComponent,
    children: [
      {
        path: 'equipment/add',
        component: EquipmentAddComponent,
      },
      {
        path: 'equipment/:id/edit',
        component: EquipmentEditComponent,
      },
      {
        path: 'equipment/:id',
        component: EquipmentDetailComponent,
      },
      {
        path: 'equipment',
        component: EquipmentComponent,
      },
      {
        path: 'reservations',
        component: ReservationComponent,
      },
      {
        path: 'reservation/add',
        component: ReservationAddComponent,
      },
      {
        path: 'reservations/:id',
        component: ReservationDetailsComponent,
      },
      {
        path: 'reservation/edit/:id',
        component: ReservationEditComponent,
      },
   
     
      {
        path: 'payments',
        component: PaymentsComponent,
      },
      {
        path: 'payment-types',
        component: PaymentTypesComponent,
      },
      
      {
        path: 'payment-types/:id/edit',
        component: PaymentTypesEditComponent,
      },
      {
        path: 'settings',
        component: EmployeeSettingsComponent,
      },
      {
        path: '',
        redirectTo: 'reservations',
        pathMatch: 'full',
      },
      {
        path: '**',
        redirectTo: 'reservations',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {}
