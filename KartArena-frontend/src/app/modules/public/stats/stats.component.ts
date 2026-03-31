import { Component } from '@angular/core';
import { MockDataService } from '../../../core/services/mock-data.service';

import {
  ApexAxisChartSeries,
  ApexNonAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexYAxis,
  ApexDataLabels,
  ApexStroke,
  ApexLegend,
  ApexTooltip,
  ApexGrid,
  ApexPlotOptions,
  ApexFill,
} from 'ng-apexcharts';

@Component({
  selector: 'app-stats',
  templateUrl: './stats.component.html',
  styleUrls: ['./stats.component.scss'],
  standalone: false
})
export class StatsComponent {
  stats: any;

  constructor(private data: MockDataService) {
    this.stats = this.data.getStatCards();
  }

  exportReport(): void {
    alert('Izvoz (demo) — ovdje kasnije ide PDF/CSV.');
  }

  // ===================== LINE (rezervacije) =====================
  lineSeries: ApexAxisChartSeries = [{ name: 'Rezervacije', data: [12, 18, 9, 22, 30, 26, 34] }];

  lineChart: ApexChart = {
    type: 'line',
    height: 240,
    toolbar: { show: false },
    foreColor: 'rgba(255,255,255,.75)',
    background: 'transparent',
  };

  lineXAxis: ApexXAxis = { categories: ['Pon', 'Uto', 'Sri', 'Čet', 'Pet', 'Sub', 'Ned'] };
  lineYAxis: ApexYAxis = {};
  lineStroke: ApexStroke = { curve: 'smooth', width: 3 };
  lineDataLabels: ApexDataLabels = { enabled: false };
  lineGrid: ApexGrid = { borderColor: 'rgba(255,255,255,.08)' };
  lineTooltip: ApexTooltip = { theme: 'dark' };
  lineLegend: ApexLegend = { show: false };

  // ===================== DONUT (tip) =====================
  donutSeries: ApexNonAxisChartSeries = [62, 38];
  donutLabels = ['Gasoline', 'Electric'];

  donutChart: ApexChart = {
    type: 'donut',
    height: 240,
    toolbar: { show: false },
    foreColor: 'rgba(255,255,255,.75)',
    background: 'transparent',
  };

  donutLegend: ApexLegend = { position: 'bottom' };

  donutPlotOptions: ApexPlotOptions = {
    pie: {
      donut: {
        size: '70%',
        labels: {
          show: true,
          total: { show: true, label: 'Ukupno' },
        },
      },
    },
  };

  donutDataLabels: ApexDataLabels = { enabled: true };
  donutTooltip: ApexTooltip = { theme: 'dark' };
  donutFill: ApexFill = { type: 'solid' };

  // ===================== BAR (staze) =====================
  barSeries: ApexAxisChartSeries = [{ name: 'Rezervacije', data: [48, 62, 39] }];

  barChart: ApexChart = {
    type: 'bar',
    height: 260,
    toolbar: { show: false },
    foreColor: 'rgba(255,255,255,.75)',
    background: 'transparent',
  };

  barXAxis: ApexXAxis = { categories: ['Sarajevo', 'Mostar', 'Tuzla'] };
  barYAxis: ApexYAxis = {};
  barDataLabels: ApexDataLabels = { enabled: false };
  barGrid: ApexGrid = { borderColor: 'rgba(255,255,255,.08)' };
  barTooltip: ApexTooltip = { theme: 'dark' };
  barLegend: ApexLegend = { show: false };
}
