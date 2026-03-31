import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';

import { KartsApiService } from '../../../api-services/karts/karts-api.service';
import { ListKartsQueryDto, ListKartsRequest } from '../../../api-services/karts/karts-api.models';
import { ToasterService } from '../../../core/services/toaster.service';
import { ConfirmDeleteDialogComponent } from './dialogs/confirm-delete/confirm-delete-dialog.component';

@Component({
  selector: 'app-karts',
  standalone: false,
  templateUrl: './karts.component.html',
  styleUrls: ['./karts.component.scss'],
})
export class KartsComponent implements OnInit {
  private api = inject(KartsApiService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);

  items: ListKartsQueryDto[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Template compatibility (HTML koristi isLoading)
  get isLoading(): boolean {
    return this.loading;
  }

  request = new ListKartsRequest();
  totalCount = 0;

  // UI state
  showOnlyEnabled = true;

  displayedColumns: string[] = ['image', 'name', 'manufacturer', 'colour', 'price', 'status', 'actions'];

  ngOnInit(): void {
    this.request.paging.pageSize = 10;
    this.request.onlyEnabled = true;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = null;

    this.request.search = this.request.search?.trim() || undefined;
    this.request.onlyEnabled = this.showOnlyEnabled;

    this.api.list(this.request).subscribe({
      next: (res) => {
        this.items = res.items || [];
        this.totalCount = res.totalItems ?? 0;
        // ako backend vraća page/pageSize, možeš ih syncati ovdje
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Karts list error', err);
        this.items = [];
        this.totalCount = 0;
        this.errorMessage = 'Greška pri učitavanju kartova';
        this.toaster.error?.(this.errorMessage) ?? alert(this.errorMessage);
        this.loading = false;
      },
      complete: () => (this.loading = false),
    });
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.load();
  }

  onPageChange(e: PageEvent): void {
    this.request.paging.page = (e.pageIndex ?? 0) + 1;
    this.request.paging.pageSize = e.pageSize ?? this.request.paging.pageSize;
    this.load();
  }

  add(): void {
    this.router.navigate(['/admin/karts/add']);
  }

  edit(id: number): void {
    this.router.navigate(['/admin/karts', id, 'edit']);
  }

  toggleEnabled(item: ListKartsQueryDto): void {
    const call = item.isEnabled ? this.api.disable(item.id) : this.api.enable(item.id);
    call.subscribe({
      next: () => {
        this.toaster.success?.('Sačuvano') ?? null;
        this.load();
      },
      error: (err: any) => {
        console.error('toggle enabled error', err);
        this.toaster.error?.('Greška') ?? alert('Greška');
      },
    });
  }

  remove(id: number): void {
    const ref = this.dialog.open(ConfirmDeleteDialogComponent);
    ref.afterClosed().subscribe((ok: boolean) => {
      if (!ok) return;
      this.api.delete(id).subscribe({
        next: () => {
          this.toaster.success?.('Obrisano') ?? null;
          this.load();
        },
        error: (err: any) => {
          console.error('delete error', err);
          this.toaster.error?.('Greška') ?? alert('Greška');
        },
      });
    });
  }

  // ---- Methods used by template (older naming) ----
  onToggleEnabledFilter(checked: boolean): void {
    this.showOnlyEnabled = checked;
    this.request.paging.page = 1;
    this.load();
  }

  onCreate(): void {
    this.add();
  }

  onEdit(item: ListKartsQueryDto): void {
    this.edit(item.id);
  }

  onDelete(item: ListKartsQueryDto): void {
    this.remove(item.id);
  }

  onToggleStatus(item: ListKartsQueryDto): void {
    this.toggleEnabled(item);
  }
}
