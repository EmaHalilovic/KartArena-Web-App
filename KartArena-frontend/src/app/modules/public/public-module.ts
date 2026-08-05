import {NgModule} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgApexchartsModule } from 'ng-apexcharts';

import {PublicRoutingModule} from './public-routing-module';

import {SharedModule} from '../shared/shared-module';

import { KartComponent } from './karts/kart.component';
import { HomeComponent } from './home/home.component';
import { StatsComponent } from './stats/stats.component';
import { KartDetailsComponent } from './kart-details/kart-details.component';
import { ReservationsComponent } from './reservations/reservations.component';
import { PaymentSuccessComponent } from '../../features/payments/payment-success/payment-success.component';
import { PaymentCancelledComponent } from '../../features/payments/payment-cancelled/payment-cancelled.component';


@NgModule({
  declarations: [
    HomeComponent,
    StatsComponent,
    KartComponent,
    KartDetailsComponent,
    ReservationsComponent,
    PaymentSuccessComponent,
    PaymentCancelledComponent
  ],
  imports: [
    NgApexchartsModule,
    SharedModule,
    FormsModule,
    PublicRoutingModule,
    FormsModule
  ]
})
export class PublicModule { }
