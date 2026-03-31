import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { ClientSettingsComponent } from './client-settings/client-settings.component';

import { ReservationComponent } from './reservation/reservation.component';
import { ReservationAddComponent } from './reservation/reservation-add/reservation-add.component';
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';

const routes: Routes = [
  {
    path: '',
    component: ClientLayoutComponent,
    children: [
      // RESERVATION
      {
        path: 'reservation',
        component: ReservationComponent,
      },
      {
        path: 'reservation/add',
        component: ReservationAddComponent,
      },
      {
        path: 'reservation/edit/:id',
        component: ReservationEditComponent,
      },

      // SETTINGS (optional)
      {
        path: 'settings',
        component: ClientSettingsComponent,
      },

      // default admin route → /client/reservation
      {
        path: '',
        redirectTo: 'reservation',
        pathMatch: 'full',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ClientRoutingModule {}
