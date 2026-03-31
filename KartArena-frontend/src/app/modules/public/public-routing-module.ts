import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './home/home.component';
import { KartComponent } from './karts/kart.component';
import { KartDetailsComponent } from './kart-details/kart-details.component';
import { StatsComponent } from './stats/stats.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
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
