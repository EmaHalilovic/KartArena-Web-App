import { Injectable } from '@angular/core';

export type StatCard = { title: string; value: string; hint: string };

@Injectable({ providedIn: 'root' })
export class MockDataService {
  getStatCards(): StatCard[] {
    return [
      { title: 'Ukupno kartova', value: '12', hint: 'Broj kartova u sistemu (demo).' },
      { title: 'Aktivni kartovi', value: '9', hint: 'Kartovi dostupni za rezervaciju.' },
      { title: 'Rezervacije sedmično', value: '151', hint: 'Ukupan broj rezervacija (demo).' },
      { title: 'Najbrža vožnja', value: '52.4s', hint: 'Najbolji zabilježeni rezultat (demo).' },
    ];
  }
}
