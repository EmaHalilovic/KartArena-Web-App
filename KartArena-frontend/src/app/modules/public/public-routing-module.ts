import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './home/home.component';
import { KartComponent } from './karts/kart.component';
import { KartDetailsComponent } from './kart-details/kart-details.component';
import { StatsComponent } from './stats/stats.component';
import { ReservationsComponent } from './reservations/reservations.component';
import { PaymentSuccessComponent } from '../../features/payments/payment-success/payment-success.component';
import { PaymentCancelledComponent } from '../../features/payments/payment-cancelled/payment-cancelled.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'reservations', component: ReservationsComponent },
  { path: 'payment/success', component: PaymentSuccessComponent },
  { path: 'payment/cancelled', component: PaymentCancelledComponent },
  { path: 'karts', component: KartComponent },
  { path: 'karts/karts/:id', component: KartDetailsComponent },
  { path: 'stats', component: StatsComponent },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PublicRoutingModule {}
