
import { NgModule } from '@angular/core';

import { AdminRoutingModule } from './admin-routing-module';
import { SharedModule } from '../shared/shared-module';

import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';

import { EquipmentComponent } from './equipment/equipment.component';
import { EquipmentAddComponent } from './equipment/equipment-add/equipment-add.component';
import { EquipmentEditComponent } from './equipment/equipment-edit/equipment-edit.component';
import { EquipmentDetailComponent } from './equipment/equipment-detail/equipment-detail.component';
import { ConfirmDeleteDialogComponent } from './equipment/dialogs/confirm-delete/confirm-delete-dialog.component';
import { EquipmentAddItemDialogComponent } from './equipment/dialogs/equipment-add-item-dialog/equipment-add-item-dialog.component';
import { EquipmentBulkAddItemsDialogComponent } from './equipment/dialogs/equipment-bulk-add-items-dialog/equipment-bulk-add-items-dialog.component';
import { EquipmentEditItemDialogComponent } from './equipment/dialogs/equipment-edit-item-dialog/equipment-edit-item-dialog.component';

import { ReservationComponent } from './reservation/reservation.component';
import { ReservationAddComponent } from './reservation/reservation-add/reservation-add.component';
import { ReservationEditComponent } from './reservation/reservation-edit/reservation-edit.component';
import { ConfirmDeleteDialogReservationComponent } from './reservation/dialogs/confirm-delete/confirm-delete-dialog.component';


import { KartsComponent } from './karts/karts.component';
import { KartsAddComponent } from './karts/karts-add/karts-add.component';
import { KartsEditComponent } from './karts/karts-edit/karts-edit.component';
import { ConfirmDeleteDialogComponent as ConfirmDeleteKartDialogComponent } from './karts/dialogs/confirm-delete/confirm-delete-dialog.component';
import { PaymentsComponent } from './payments/payments.component';


@NgModule({
  declarations: [
    AdminLayoutComponent,
    AdminSettingsComponent,
    EquipmentComponent,
    EquipmentAddComponent,
    EquipmentEditComponent,
    EquipmentDetailComponent,
    ConfirmDeleteDialogComponent,
    EquipmentAddItemDialogComponent,
    EquipmentBulkAddItemsDialogComponent,
    EquipmentEditItemDialogComponent,
    ReservationComponent,
    ReservationAddComponent,
    ReservationEditComponent,
    ConfirmDeleteDialogReservationComponent,
     KartsComponent,
    KartsAddComponent,
    KartsEditComponent,
    ConfirmDeleteKartDialogComponent,
    PaymentsComponent,
  ],
  imports: [
    AdminRoutingModule,
    SharedModule,
  ],
})
export class AdminModule {}
