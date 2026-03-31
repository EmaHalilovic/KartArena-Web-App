import { NgModule } from '@angular/core';

import { ClientRoutingModule } from './client-routing-module';
import { SharedModule } from '../shared/shared-module';

import { ClientLayoutComponent } from './client-layout/client-layout.component';
import { ClientSettingsComponent } from './client-settings/client-settings.component';

import { ReservationComponent } from './reservation/reservation.component';
import { ReservationAddComponent } from './reservation/reservation-add/reservation-add.component';
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';
import { ConfirmDeleteDialogComponent } from './reservation/dialogs/confirm-delete/confirm-delete-dialog.component';

@NgModule({
  declarations: [
    ClientLayoutComponent,
    ClientSettingsComponent,
    ReservationComponent,
    ReservationAddComponent,
    ReservationEditComponent,
    ConfirmDeleteDialogComponent,
  ],
  imports: [
    ClientRoutingModule,
    SharedModule,
  ],
})
export class ClientModule {}
