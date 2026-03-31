import {NgModule} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgApexchartsModule } from 'ng-apexcharts';

import {PublicRoutingModule} from './public-routing-module';

import {SharedModule} from '../shared/shared-module';

import { KartComponent } from './karts/kart.component';
import { HomeComponent } from './home/home.component';
import { StatsComponent } from './stats/stats.component';
import { KartDetailsComponent } from './kart-details/kart-details.component';


@NgModule({
  declarations: [
    HomeComponent,
    StatsComponent,
    KartComponent,
    KartDetailsComponent
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
