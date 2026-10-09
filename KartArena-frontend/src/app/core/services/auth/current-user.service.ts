import { Injectable, inject, computed } from '@angular/core';
import { AuthFacadeService } from './auth-facade.service';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {

  private auth = inject(AuthFacadeService);

  /** Read-only signal that can be consumed by the UI. */
  currentUser = computed(() => this.auth.currentUser());

  isAuthenticated = computed(() => this.auth.isAuthenticated());

  isAdmin = computed(() => this.auth.isAdmin());

  isManager = computed(() => this.auth.isManager());

  isEmployee = computed(() => this.auth.isEmployee());

  get snapshot() {
    return this.auth.currentUser();
  }

  /** Returns the default route based on the authenticated user's role. */
  getDefaultRoute(): string {
    const user = this.snapshot;

    if (!user) return '/auth/login';

    if (user.isAdmin) return '/admin';

    if (user.isEmployee) return '/employee';

    return '/auth/login';
  }
}