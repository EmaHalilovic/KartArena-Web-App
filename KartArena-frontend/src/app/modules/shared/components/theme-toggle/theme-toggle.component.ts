import { Component, inject } from '@angular/core';
import { ThemeService } from '../../../../core/services/theme/theme.service';

@Component({
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.component.html',
  styleUrl: './theme-toggle.component.scss',
  standalone: false
})
export class ThemeToggleComponent {
  readonly themeService = inject(ThemeService);
}
