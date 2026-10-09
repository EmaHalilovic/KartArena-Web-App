import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, tap, catchError, map, finalize } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

import { AuthApiService } from '../../../api-services/auth/auth-api.service';
import {
  LoginCommand,
  LoginCommandDto,
  LogoutCommand,
  RefreshTokenCommand,
  RefreshTokenCommandDto,
} from '../../../api-services/auth/auth-api.model';

import { AuthStorageService } from './auth-storage.service';
import { CurrentUserDto } from './current-user.dto';
import { JwtPayloadDto } from './jwt-payload.dto';

/**
 * Main authentication facade.
 * Handles authentication API calls, token storage,
 * JWT decoding and the current user state.
 */
@Injectable({ providedIn: 'root' })
export class AuthFacadeService {
  private api = inject(AuthApiService);
  private storage = inject(AuthStorageService);
  private router = inject(Router);

  private _currentUser = signal<CurrentUserDto | null>(null);

  currentUser = this._currentUser.asReadonly();

  isAuthenticated = computed(() => !!this._currentUser());
  isAdmin = computed(() => this._currentUser()?.isAdmin ?? false);
  isManager = computed(() => this._currentUser()?.isManager ?? false);
  isEmployee = computed(() => this._currentUser()?.isEmployee ?? false);

  constructor() {
    this.initializeFromToken();
  }

  /**
   * Authenticates the user and stores the received tokens.
   */
  login(payload: LoginCommand): Observable<void> {
    return this.api.login(payload).pipe(
      tap((response: LoginCommandDto) => {
        this.storage.saveLogin(response);
        this.decodeAndSetUser(response.accessToken);
      }),
      map(() => void 0)
    );
  }

  /**
   * Logs out the user on the server and clears the local authentication state.
   */
  logout(): Observable<void> {
    const refreshToken = this.storage.getRefreshToken();

    if (!refreshToken) {
      this.clearUserState();
      return of(void 0);
    }

    const payload: LogoutCommand = { refreshToken };

    return this.api.logout(payload).pipe(
      catchError(() => of(void 0)),
      finalize(() => this.clearUserState())
    );
  }

  /**
   * Refreshes the access token using the stored refresh token.
   */
  refresh(payload: RefreshTokenCommand): Observable<RefreshTokenCommandDto> {
    return this.api.refresh(payload).pipe(
      tap((response: RefreshTokenCommandDto) => {
        this.storage.saveRefresh(response);
        this.decodeAndSetUser(response.accessToken);
      })
    );
  }

  /**
   * Clears the authentication state and redirects to the login page.
   */
  redirectToLogin(): void {
    this.clearUserState();
    this.router.navigate(['/auth/login']);
  }

  /**
   * Returns the current access token.
   */
  getAccessToken(): string | null {
    return this.storage.getAccessToken();
  }

  /**
   * Returns the current refresh token.
   */
  getRefreshToken(): string | null {
    return this.storage.getRefreshToken();
  }

  /**
   * Restores the current user state from the stored access token.
   */
  private initializeFromToken(): void {
    const token = this.storage.getAccessToken();

     if (!token) return;

   try {
    this.decodeAndSetUser(token);
  } catch {
    // decodeAndSetUser has cleared the state if the token is invalid.
  }
  }

  /**
   * Decodes the JWT and updates the current user state.
   */
  private decodeAndSetUser(token: string): void {
    try {
      const payload = jwtDecode<JwtPayloadDto>(token);

      const userId = Number(payload.sub);
    const email =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    const role =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    const allowedRoles = ['Admin', 'Employee', 'User'];

    if (
      !Number.isSafeInteger(userId) ||
      userId <= 0 ||
      typeof email !== 'string' ||
      !email.trim() ||
      !allowedRoles.includes(role)
    ) {
      throw new Error('Invalid JWT claims.');
    }

    const user: CurrentUserDto = {
      userId,
      email,
      isAdmin: role === 'Admin',
      isManager: role === 'Manager',
      isEmployee: role === 'Employee',
    };

    this._currentUser.set(user);
   
  } catch {
    this.clearUserState();
      throw new Error('Unvalid token or unsupported role.');
  }
}

  /**
   * Clears the current user and all stored authentication tokens.
   */
  private clearUserState(): void {
    this._currentUser.set(null);
    this.storage.clear();
  }
}