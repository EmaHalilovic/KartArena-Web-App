
import { NgModule } from '@angular/core';

import { AdminRoutingModule } from './admin-routing-module';
import { SharedModule } from '../shared/shared-module';

import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';
import { ReportsComponent } from './reports/reports.component';

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
import { ReservationDetailsComponent } from './reservation/reservation-details/reservation-details.component';
import { ConfirmDeleteDialogReservationComponent } from './reservation/dialogs/confirm-delete/confirm-delete-dialog.component';
import { ReservationCashPaymentDialogComponent } from './reservation/dialogs/reservation-cash-payment-dialog/reservation-cash-payment-dialog.component';
import { ReservationAssignDialogComponent } from './reservation/dialogs/reservation-assign-dialog/reservation-assign-dialog.component';


import { KartsComponent } from './karts/karts.component';
import { KartsAddComponent } from './karts/karts-add/karts-add.component';
import { KartsEditComponent } from './karts/karts-edit/karts-edit.component';
import { ConfirmDeleteDialogComponent as ConfirmDeleteKartDialogComponent } from './karts/dialogs/confirm-delete/confirm-delete-dialog.component';
import { PaymentsComponent } from './payments/payments.component';
import { PaymentTypesComponent } from './payment-types/payment-types.component';
import { PaymentTypesAddComponent } from './payment-types/payment-types-add/payment-types-add.component';
import { PaymentTypesEditComponent } from './payment-types/payment-types-edit/payment-types-edit.component';
import { ConfirmDeletePaymentTypeDialogComponent } from './payment-types/dialogs/confirm-delete/confirm-delete-dialog.component';


@NgModule({
  declarations: [
    AdminLayoutComponent,
    AdminSettingsComponent,
    ReportsComponent,
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
    ReservationDetailsComponent,
    ConfirmDeleteDialogReservationComponent,
    ReservationCashPaymentDialogComponent,
    ReservationAssignDialogComponent,
     KartsComponent,
    KartsAddComponent,
    KartsEditComponent,
    ConfirmDeleteKartDialogComponent,
    PaymentsComponent,
    PaymentTypesComponent,
    PaymentTypesAddComponent,
    PaymentTypesEditComponent,
    ConfirmDeletePaymentTypeDialogComponent,
  ],
  imports: [
    AdminRoutingModule,
    SharedModule,
  ],
})
export class AdminModule {}
