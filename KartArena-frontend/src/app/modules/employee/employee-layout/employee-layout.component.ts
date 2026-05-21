import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AuthFacadeService } from '../../../core/services/auth/auth-facade.service';

@Component({
  selector: 'app-employee-layout',
  standalone: false,
  templateUrl: './employee-layout.component.html',
  styleUrl: './employee-layout.component.scss'
})
export class EmployeeLayoutComponent {
  private translate = inject(TranslateService);
  auth = inject(AuthFacadeService);

  sidebarOpened = true;
  currentLang: string;

  languages = [
    { code: 'bs', name: 'Bosanski', label: 'BS' },
    { code: 'en', name: 'English', label: 'EN' }
  ];

  constructor() {
    this.currentLang = this.translate.currentLang || 'bs';
  }

  switchLanguage(langCode: string): void {
    this.currentLang = langCode;
    this.translate.use(langCode);
    localStorage.setItem('language', langCode);
  }

  toggleSidebar(): void {
    this.sidebarOpened = !this.sidebarOpened;
  }

  getCurrentLanguage() {
    return this.languages.find(lang => lang.code === this.currentLang);
  }
}
