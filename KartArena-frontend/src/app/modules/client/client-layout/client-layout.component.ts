import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

import { AuthFacadeService } from '../../../core/services/auth/auth-facade.service';

@Component({
  selector: 'app-client-layout',
  standalone: false,
  templateUrl: './client-layout.component.html',
  styleUrl: './client-layout.component.scss'
})
export class ClientLayoutComponent {
  private translate = inject(TranslateService);
  private router = inject(Router);

  auth = inject(AuthFacadeService);

  currentLang: string;

  languages = [
    { code: 'bs', name: 'Bosanski', flag: '🇧🇦' },
    { code: 'en', name: 'English', flag: '🇬🇧' }
  ];

  constructor() {
    this.currentLang = this.translate.currentLang || 'bs';
  }

  switchLanguage(langCode: string): void {
    this.currentLang = langCode;
    this.translate.use(langCode);
    localStorage.setItem('language', langCode);
  }

  getCurrentLanguage() {
    return this.languages.find(lang => lang.code === this.currentLang);
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      }
    });
  }
}