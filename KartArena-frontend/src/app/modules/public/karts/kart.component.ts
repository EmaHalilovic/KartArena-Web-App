import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { KartsApiService } from '../../../api-services/karts/karts-api.service';
import { ListKartsQueryDto } from '../../../api-services/karts/karts-api.models';

type SortKey = 'name' | 'price';
type SortDir = 'asc' | 'desc';

type QuickFilter =
  | { kind: 'all' }
  | { kind: 'powertrain'; id: number }
  | { kind: 'enabled'; only: boolean };

@Component({
  selector: 'app-karts',
  standalone: false,
  template: `
    <section class="karts-page">
      <!-- HEADER (uvijek gore, full width) -->
      <app-header class="hdr"></app-header>

      <div class="container">
        <div class="layout">
          <!-- LEFT STRIP -->
          <aside class="side">
            <div class="vertical">PONUDA KARTOVA</div>
          </aside>

          <!-- MAIN -->
          <main class="main">
            <div class="panel">
              <!-- TOP BAR -->
              <div class="topbar">
                <form class="search" (submit)="onSearch($event)">
                  <span class="sicon" aria-hidden="true">⌕</span>
                  <input
                    type="text"
                    [(ngModel)]="q"
                    name="q"
                    placeholder="Pretraži kartove (naziv, proizvođač, boja)..."
                    aria-label="Pretraži kartove"
                  />
                  <button class="sbtn" type="submit">Traži</button>
                </form>

                <button class="btn ghost" type="button" (click)="reset()">Reset</button>
              </div>

              <!-- QUICK FILTERS + SORT -->
              <div class="filters">
                <div class="left">
                  <button
                    class="pill"
                    [class.active]="filter.kind === 'all'"
                    (click)="setFilter({ kind: 'all' })"
                    type="button"
                  >
                    Sve
                  </button>

                  <button
                    class="pill"
                    [class.active]="filter.kind === 'enabled' && filter.only"
                    (click)="setFilter({ kind: 'enabled', only: true })"
                    type="button"
                  >
                    Samo aktivni
                  </button>

                  <button
                    class="pill"
                    [class.active]="filter.kind === 'powertrain' && filter.id === 1"
                    (click)="setFilter({ kind: 'powertrain', id: 1 })"
                    type="button"
                  >
                    2T
                  </button>

                  <button
                    class="pill"
                    [class.active]="filter.kind === 'powertrain' && filter.id === 2"
                    (click)="setFilter({ kind: 'powertrain', id: 2 })"
                    type="button"
                  >
                    4T
                  </button>

                  <button
                    class="pill"
                    [class.active]="filter.kind === 'powertrain' && filter.id === 3"
                    (click)="setFilter({ kind: 'powertrain', id: 3 })"
                    type="button"
                  >
                    Električni
                  </button>
                </div>

                <div class="right">
                  <span class="lbl">Sort:</span>

                  <button
                    class="pill ghost"
                    [class.active]="sortKey === 'name'"
                    type="button"
                    (click)="setSort('name')"
                  >
                    Naziv
                  </button>

                  <button
                    class="pill ghost"
                    [class.active]="sortKey === 'price'"
                    type="button"
                    (click)="setSort('price')"
                  >
                    Cijena
                  </button>

                  <button class="pill ghost" type="button" (click)="toggleDir()">
                    {{ sortDir === 'asc' ? 'ASC ↑' : 'DESC ↓' }}
                  </button>
                </div>
              </div>

              <!-- GRID -->
              <div class="grid" *ngIf="!loading; else loadingTpl">
                <div class="empty" *ngIf="viewKarts.length === 0">
                  Nema kartova za prikaz.
                </div>

                <article class="card" *ngFor="let k of viewKarts" (click)="openDetails(k.id)">
                  <div class="img-wrap">
                    <img
                      class="img"
                      [src]="k.imageUrl || 'https://placehold.co/600x400'"
                      alt="Kart image"
                    />
                  </div>

                  <div class="body">
                    <div class="title-row">
                      <h3 class="title">{{ k.name }}</h3>
                      <span class="badge" *ngIf="k.pricePerSession != null">{{ k.pricePerSession }} KM</span>
                    </div>

                    <div class="meta">
                      <span>{{ k.manufacturer || '—' }}</span>
                      <span class="dot">•</span>
                      <span>{{ k.colour || '—' }}</span>
                    </div>

                    <p class="desc" *ngIf="k.description">
                      {{ k.description }}
                    </p>

                    <div class="cta">
                      <span class="link">Detalji</span>
                      <span class="arrow">→</span>
                    </div>
                  </div>
                </article>
              </div>

              <ng-template #loadingTpl>
                <div class="loading">Učitavanje...</div>
              </ng-template>
            </div>
          </main>
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host { display:block; }

    /* PAGE */
    .karts-page{
      min-height: 100vh;
      background: radial-gradient(1200px 700px at 65% -10%, rgba(255,0,0,.12), transparent 60%),
                  radial-gradient(900px 600px at 10% 20%, rgba(255,255,255,.06), transparent 60%),
                  #000;
      color:#fff;
      padding-bottom: 46px;
    }
    .hdr{ display:block; }

    .container{
      max-width: 1200px;
      margin: 0 auto;
      padding: 18px 18px 0;
    }

    .layout{
      display:grid;
      grid-template-columns: 84px 1fr;
      gap: 22px;
      align-items:start;
      margin-top: 10px;
    }

    /* SIDE STRIP */
    .side{
      position: sticky;
      top: 92px; /* da stoji ispod headera */
      height: calc(100vh - 110px);
      display:flex;
      align-items:center;
      justify-content:center;
      opacity:.95;
    }
    .vertical{
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      font-weight: 900;
      letter-spacing: 6px;
      font-size: 54px;
      color: rgba(255,255,255,.82);
      text-transform: uppercase;
      user-select:none;
    }

    /* PANEL */
    .panel{
      background: rgba(70,70,70,.92);
      border: 1px solid rgba(255,255,255,.10);
      border-radius: 16px;
      padding: 18px;
      box-shadow:
        0 30px 80px rgba(0,0,0,.55),
        inset 0 1px 0 rgba(255,255,255,.06);
      backdrop-filter: blur(10px);
    }

    /* TOPBAR */
    .topbar{
      display:flex;
      gap: 12px;
      align-items:center;
      justify-content:space-between;
      padding-bottom: 12px;
      border-bottom: 1px solid rgba(255,255,255,.10);
      margin-bottom: 12px;
    }

    .search{
      flex:1;
      display:flex;
      align-items:center;
      gap: 10px;
      background: rgba(255,255,255,.96);
      border-radius: 999px;
      padding: 10px 10px 10px 14px;
      box-shadow: inset 0 0 0 2px rgba(0,0,0,.08);
    }
    .sicon{ font-weight:900; color: rgba(0,0,0,.55); }
    .search input{
      border:none; outline:none;
      width:100%;
      background: transparent;
      font-weight: 800;
    }
    .sbtn{
      border:none; cursor:pointer;
      border-radius: 999px;
      padding: 10px 14px;
      font-weight: 900;
      background: #111;
      color:#fff;
      transition: transform .12s ease, opacity .12s ease;
    }
    .sbtn:hover{ transform: translateY(-1px); opacity:.95; }

    .btn{
      border:none; cursor:pointer;
      border-radius: 12px;
      padding: 10px 14px;
      font-weight: 900;
      background:#111;
      color:#fff;
    }
    .btn.ghost{
      background: transparent;
      outline: 1px solid rgba(255,255,255,.25);
      color: rgba(255,255,255,.95);
    }
    .btn.ghost:hover{ outline-color: rgba(255,255,255,.38); }

    /* FILTERS */
    .filters{
      display:flex;
      align-items:center;
      justify-content:space-between;
      gap: 14px;
      flex-wrap: wrap;
      padding: 2px 2px 14px;
    }
    .left, .right{
      display:flex;
      align-items:center;
      gap: 10px;
      flex-wrap: wrap;
    }
    .lbl{ font-weight: 900; opacity:.85; }

    .pill{
      border:none; cursor:pointer;
      border-radius: 999px;
      padding: 10px 14px;
      font-weight: 900;
      background: rgba(0,0,0,.32);
      color: rgba(255,255,255,.92);
      transition: transform .12s ease, background-color .12s ease, color .12s ease;
    }
    .pill:hover{ transform: translateY(-1px); }
    .pill.active{ background:#fff; color:#000; }
    .pill.ghost{
      background: transparent;
      outline: 1px solid rgba(255,255,255,.22);
    }
    .pill.ghost.active{
      outline-color: rgba(255,255,255,.45);
      background: rgba(255,255,255,.10);
      color:#fff;
    }

    /* GRID + CARD */
    .grid{
      display:grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 14px;
      padding-top: 4px;
    }
    .card{
      border-radius: 16px;
      overflow:hidden;
      background: rgba(0,0,0,.40);
      border: 1px solid rgba(255,255,255,.14);
      cursor:pointer;
      transition: transform .14s ease, border-color .14s ease, box-shadow .14s ease;
      box-shadow: 0 10px 30px rgba(0,0,0,.30);
    }
    .card:hover{
      transform: translateY(-3px);
      border-color: rgba(255,255,255,.28);
      box-shadow: 0 18px 45px rgba(0,0,0,.45);
    }
    .img{
      width:100%;
      height: 170px;
      object-fit: cover;
      display:block;
      background: rgba(255,255,255,.06);
    }
    .body{ padding: 12px 12px 14px; }
    .title-row{
      display:flex;
      align-items:flex-start;
      justify-content:space-between;
      gap: 10px;
    }
    .title{
      margin: 0;
      font-size: 16px;
      font-weight: 900;
      letter-spacing: .2px;
      line-height: 1.2;
    }
    .badge{
      font-weight: 900;
      font-size: 12px;
      padding: 6px 10px;
      border-radius: 999px;
      background: rgba(255,255,255,.10);
      border: 1px solid rgba(255,255,255,.16);
      white-space: nowrap;
    }
    .meta{
      margin-top: 8px;
      display:flex;
      gap: 8px;
      font-weight: 800;
      color: rgba(255,255,255,.80);
      font-size: 13px;
    }
    .dot{ opacity:.6; }
    .desc{
      margin: 10px 0 0;
      opacity: .90;
      font-weight: 700;
      font-size: 13px;
      line-height: 1.35;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .cta{
      margin-top: 12px;
      display:flex;
      align-items:center;
      justify-content:space-between;
      font-weight: 900;
    }
    .link{ text-decoration: underline; }
    .arrow{ opacity:.85; }

    .loading{ padding: 26px 6px; font-weight: 900; opacity: .9; }
    .empty{
      grid-column: 1/-1;
      text-align:center;
      padding: 30px 10px;
      font-weight: 900;
      opacity: .9;
    }

    /* RESPONSIVE */
    @media (max-width: 1100px){
      .grid{ grid-template-columns: repeat(2, minmax(0, 1fr)); }
    }
    @media (max-width: 820px){
      .layout{ grid-template-columns: 1fr; }
      .side{
        position: static;
        height: auto;
        justify-content:flex-start;
        padding: 6px 0 0;
      }
      .vertical{
        writing-mode: initial;
        transform: none;
        font-size: 28px;
        letter-spacing: 3px;
      }
    }
    @media (max-width: 720px){
      .grid{ grid-template-columns: 1fr; }
      .topbar{ flex-direction: column; align-items: stretch; }
      .btn.ghost{ width: fit-content; margin-left: auto; }
    }
  `],
})
export class KartComponent implements OnInit {
  karts: ListKartsQueryDto[] = [];
  viewKarts: ListKartsQueryDto[] = [];
  loading = false;

  q = '';
  onlyEnabled = true;

  filter: QuickFilter = { kind: 'enabled', only: true };
  sortKey: SortKey = 'name';
  sortDir: SortDir = 'asc';

  constructor(private kartsApi: KartsApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadKarts();
  }

  onSearch(e: Event): void {
    e.preventDefault();
    this.loadKarts();
  }

  setFilter(f: QuickFilter): void {
    this.filter = f;

    if (f.kind === 'enabled') {
      this.onlyEnabled = f.only;
      this.loadKarts();
      return;
    }

    this.applyLocal();
  }

  setSort(key: SortKey): void {
    this.sortKey = key;
    this.applyLocal();
  }

  toggleDir(): void {
    this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    this.applyLocal();
  }

  reset(): void {
    this.q = '';
    this.onlyEnabled = true;
    this.filter = { kind: 'enabled', only: true };
    this.sortKey = 'name';
    this.sortDir = 'asc';
    this.loadKarts();
  }

  loadKarts(): void {
    this.loading = true;
    this.kartsApi
      .list({
        search: this.q?.trim() || undefined,
        onlyEnabled: this.onlyEnabled,
        paging: { page: 1, pageSize: 60 },
      })
      .subscribe({
        next: (res) => {
          this.karts = res.items || [];
          this.applyLocal();
        },
        error: (err) => {
          console.error('Karts list error:', err);
          this.karts = [];
          this.viewKarts = [];
        },
        complete: () => (this.loading = false),
      });
  }

  private applyLocal(): void {
    let items = [...(this.karts || [])];

    if (this.filter.kind === 'powertrain') {
      // @ts-ignore
      items = items.filter((x: any) => x.powertrainTypeId === this.filter.id);
    }

    const dir = this.sortDir === 'asc' ? 1 : -1;
    items.sort((a: any, b: any) => {
      if (this.sortKey === 'price') {
        const ap = a.pricePerSession ?? -1;
        const bp = b.pricePerSession ?? -1;
        return (ap - bp) * dir;
      }
      const an = (a.name || '').toString().toLowerCase();
      const bn = (b.name || '').toString().toLowerCase();
      return an.localeCompare(bn) * dir;
    });

    this.viewKarts = items;
  }

  openDetails(id: number): void {
    this.router.navigate(['/karts', id]);
  }
}
