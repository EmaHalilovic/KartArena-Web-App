import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, signal } from '@angular/core';

export type AppTheme = 'dark' | 'light';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'kart-arena-theme';
  private readonly currentTheme = signal<AppTheme>(this.getInitialTheme());

  readonly theme = this.currentTheme.asReadonly();

  constructor(@Inject(DOCUMENT) private readonly document: Document) {
    this.applyTheme(this.currentTheme());
  }

  toggle(): void {
    this.setTheme(this.currentTheme() === 'dark' ? 'light' : 'dark');
  }

  setTheme(theme: AppTheme): void {
    this.currentTheme.set(theme);
    localStorage.setItem(this.storageKey, theme);
    this.applyTheme(theme);
  }

  private getInitialTheme(): AppTheme {
    const savedTheme = localStorage.getItem(this.storageKey);

    if (savedTheme === 'dark' || savedTheme === 'light') {
      return savedTheme;
    }

    return window.matchMedia?.('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
  }

  private applyTheme(theme: AppTheme): void {
    const oppositeTheme: AppTheme = theme === 'dark' ? 'light' : 'dark';
    const elements = [this.document.documentElement, this.document.body];

    elements.forEach(element => {
      element.classList.remove(`${oppositeTheme}-theme`);
      element.classList.add(`${theme}-theme`);
      element.style.colorScheme = theme;
    });
  }
}
