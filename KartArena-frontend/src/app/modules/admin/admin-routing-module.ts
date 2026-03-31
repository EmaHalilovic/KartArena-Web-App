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
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';


import { KartsComponent } from './karts/karts.component';
import { KartsAddComponent } from './karts/karts-add/karts-add.component';
import { KartsEditComponent } from './karts/karts-edit/karts-edit.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
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
        path: 'reservation/edit/:id',
        component: ReservationEditComponent,
      },

 //PAYMENTS
       {
        path: 'payments',
        component: PaymentsComponent,
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

      // SETTINGS (optional)
      {
        path: 'settings',
        component: AdminSettingsComponent,
      },

      // default admin route → /admin/equipment
      {
        path: '',
        redirectTo: 'karts',
        pathMatch: 'full',
      },
      {
        path: '**',
        redirectTo: 'karts',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
