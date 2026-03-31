import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { KartsApiService } from '../../../api-services/karts/karts-api.service';
import { GetKartByIdQueryDto } from '../../../api-services/karts/karts-api.models';

@Component({
  selector: 'app-kart-details',
  standalone: false,
  
  template: `
    <section class="details">
      <div class="container" *ngIf="kart; else loadingTpl">
        <button class="back" (click)="back()">← Nazad</button>

        <div class="hero">
          <img class="img" [src]="kart.imageUrl || 'https://placehold.co/1200x700'" alt="Kart image" />
        </div>

        <div class="content">
          <h1>{{ kart.name }}</h1>
          <div class="meta">
            <span>{{ kart.manufacturer || '—' }}</span>
            <span class="dot">•</span>
            <span>{{ kart.colour || '—' }}</span>
            <span class="dot">•</span>
            <span *ngIf="kart.yearOfManufacture">{{ kart.yearOfManufacture }}</span>
          </div>

          <div class="price" *ngIf="kart.pricePerSession != null">
            {{ kart.pricePerSession }} KM / vožnja
          </div>

          <p class="desc" *ngIf="kart.description">{{ kart.description }}</p>

          <div class="specs">
            <div class="spec"><span>Šasija</span><b>{{ kart.chassisNumber || '—' }}</b></div>
            <div class="spec"><span>Powertrain</span><b>{{ kart.powertrainTypeName || '—' }}</b></div>
          </div>
        </div>
      </div>

      <ng-template #loadingTpl>
        <div class="loading container">Učitavanje...</div>
      </ng-template>
    </section>
  `,

  styles: [
    `
      .details {
        min-height: calc(100vh - 74px);
        background: #000;
        color: #fff;
        padding: 38px 0 46px;
      }
      .content {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 26px;
        align-items: start;
      }
      @media (max-width: 900px) {
        .content {
          grid-template-columns: 1fr;
        }
      }
      .h1 {
        margin: 0 0 14px;
        font-weight: 900;
        letter-spacing: 1px;
        font-size: 48px;
        text-transform: uppercase;
      }
      .meta {
        display: grid;
        gap: 8px;
        background: rgba(255, 255, 255, 0.06);
        border: 1px solid rgba(255, 255, 255, 0.14);
        border-radius: 12px;
        padding: 14px;
      }
      .meta span {
        display: inline-block;
        min-width: 120px;
        opacity: 0.75;
        font-weight: 700;
      }
      .panel {
        border-radius: 14px;
        padding: 18px;
        background: #4a4a4a;
        box-shadow: 0 0 50px rgba(0, 0, 0, 0.45);
      }
      .p-title {
        font-weight: 900;
        letter-spacing: 0.5px;
        font-size: 18px;
      }
      .p-body {
        margin-top: 10px;
        color: rgba(255, 255, 255, 0.9);
        line-height: 1.5;
      }
      .btn {
        margin-top: 14px;
        border: none;
        border-radius: 999px;
        padding: 10px 14px;
        font-weight: 900;
        cursor: pointer;
      }
      .notfound {
        text-align: center;
        padding: 40px 12px;
      }
    `,
  ],
})
export class KartDetailsComponent implements OnInit {
  kart: GetKartByIdQueryDto | null = null;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private kartsApi: KartsApiService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.kart = null;
      return;
    }

    this.loading = true;
    this.kartsApi.getById(id).subscribe({
      next: (res) => (this.kart = res),
      error: (err) => {
        console.error('Kart details error:', err);
        this.kart = null;
      },
      complete: () => (this.loading = false),
    });
  }

  back(): void {
    this.router.navigate(['/karts']);
  }
}
