import { Injectable } from '@angular/core';

export type StatCard = { title: string; value: string; hint: string };

@Injectable({ providedIn: 'root' })
export class MockDataService {
  getStatCards(): StatCard[] {
    return [
      { title: 'Total karts', value: '12', hint: 'Number of karts in the system (demo).' },
      { title: 'Active karts', value: '9', hint: 'Karts available for reservation.' },
      { title: 'Weekly reservations', value: '151', hint: 'Total number of reservations (demo).' },
      { title: 'Fastest ride', value: '52.4s', hint: 'Best recorded result (demo).' },
    ];
  }
}
