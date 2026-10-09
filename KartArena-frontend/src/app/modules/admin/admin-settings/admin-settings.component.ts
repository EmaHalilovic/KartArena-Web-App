import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AuthFacadeService } from '../../../core/services/auth/auth-facade.service';
import { AppTheme, ThemeService } from '../../../core/services/theme/theme.service';

@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.scss',
})
export class AdminSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthFacadeService);
  private readonly themeService = inject(ThemeService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);

  accountNoticeVisible = false;
  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(50)]],
  });
  readonly passwordForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [passwordMatchValidator] }
  );
  readonly preferencesForm = this.fb.nonNullable.group({
    theme: [this.themeService.theme(), [Validators.required]],
    language: [this.translate.currentLang || localStorage.getItem('language') || 'bs', [Validators.required]],
  });

  ngOnInit(): void {
    this.profileForm.controls.email.setValue(this.auth.currentUser()?.email ?? '');
  }

  showAccountNotice(): void { this.accountNoticeVisible = true; }
  setTheme(theme: AppTheme): void { this.preferencesForm.controls.theme.setValue(theme); this.themeService.setTheme(theme); }
  setLanguage(language: string): void { this.preferencesForm.controls.language.setValue(language); this.translate.use(language); localStorage.setItem('language', language); }
  navigate(path: string): void { this.router.navigate([path]); }
}

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPassword && confirmPassword && newPassword !== confirmPassword ? { passwordMismatch: true } : null;
}
