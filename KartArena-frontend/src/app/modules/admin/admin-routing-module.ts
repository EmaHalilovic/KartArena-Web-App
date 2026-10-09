import { PaymentsComponent } from './payments/payments.component';

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';

import { EquipmentComponent } from './equipment/equipment.component';
import { EquipmentAddComponent } from './equipment/equipment-add/equipment-add.component';
import { EquipmentEditComponent } from './equipment/equipment-edit/equipment-edit.component';
import { EquipmentDetailComponent } from './equipment/equipment-detail/equipment-detail.component';

import { ReservationComponent } from './reservation/reservation.component';
import { ReservationAddComponent } from './reservation/reservation-add/reservation-add.component';
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';
import { ReservationDetailsComponent } from './reservation/reservation-details/reservation-details.component';


import { KartsComponent } from './karts/karts.component';
import { KartsAddComponent } from './karts/karts-add/karts-add.component';
import { KartsEditComponent } from './karts/karts-edit/karts-edit.component';
import { PaymentTypesComponent } from './payment-types/payment-types.component';
import { PaymentTypesAddComponent } from './payment-types/payment-types-add/payment-types-add.component';
import { PaymentTypesEditComponent } from './payment-types/payment-types-edit/payment-types-edit.component';
import { ReportsComponent } from './reports/reports.component';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
      },
      // EQUIPMENT
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
      //RESERVATIONS
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

 //PAYMENTS
      {
        path: 'payments',
        component: PaymentsComponent,
      },
      {
        path: 'payment-types',
        component: PaymentTypesComponent,
      },
      {
        path: 'payment-types/add',
        component: PaymentTypesAddComponent,
      },
      {
        path: 'payment-types/:id/edit',
        component: PaymentTypesEditComponent,
      },
      // KARTS
      {
        path: 'karts',
        component: KartsComponent,
      },
      {
        path: 'karts/add',
        component: KartsAddComponent,
      },
      {
        path: 'karts/:id/edit',
        component: KartsEditComponent,
      },
  {
        path: 'reports',
        component: ReportsComponent,
      },
      // SETTINGS (optional)
      {
        path: 'settings',
        component: AdminSettingsComponent,
      },

      // default admin route → /admin/equipment
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: '**',
        redirectTo: 'dashboard',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
