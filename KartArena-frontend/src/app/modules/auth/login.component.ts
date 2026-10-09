import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthFacadeService } from '../../core/services/auth/auth-facade.service';
import { CurrentUserService } from '../../core/services/auth/current-user.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  template: `
    <main class="login-page">
      <form (ngSubmit)="submit()" class="login-card">
        <h1>Sign in</h1>

        <label>
          Email
          <input
            type="email"
            name="email"
            [(ngModel)]="email"
            required
          />
        </label>

        <label>
          Password
          <input
            type="password"
            name="password"
            [(ngModel)]="password"
            required
          />
        </label>

        <button type="submit" [disabled]="loading">
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>

        <p *ngIf="error" role="alert">
          {{ error }}
        </p>
      </form>
    </main>
  `,
  styles: [`
    .login-page {
      min-height: 70vh;
      display: grid;
      place-items: center;
      padding: 2rem;
    }

    .login-card {
      width: min(420px, 100%);
      display: grid;
      gap: 1rem;
      padding: 2rem;
      border: 1px solid #ddd;
      border-radius: 12px;
    }

    label {
      display: grid;
      gap: .4rem;
    }

    input {
      padding: .75rem;
    }

    button {
      padding: .8rem;
      cursor: pointer;
    }
  `]
})
export class LoginComponent {
  private auth = inject(AuthFacadeService);
  private currentUser = inject(CurrentUserService);
  private router = inject(Router);

  email = '';
  password = '';
  loading = false;
  error = '';

  submit(): void {
    this.loading = true;
    this.error = '';

    this.auth.login({
      email: this.email,
      password: this.password,
      fingerprint: null
    }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl(
          this.currentUser.getDefaultRoute()
        );
      },
      error: () => {
        this.loading = false;
        this.error = 'Invalid email or password.';
      }
    });
  }
}