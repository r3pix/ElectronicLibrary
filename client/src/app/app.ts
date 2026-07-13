import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from './core/services/auth.service';
import { LanguageService, SupportedLanguage } from './core/services/language.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    TranslatePipe
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly authService = inject(AuthService);
  protected readonly languageService = inject(LanguageService);
  protected readonly themeService = inject(ThemeService);
  private readonly router = inject(Router);

  protected readonly displayName = computed(() => {
    const user = this.authService.currentUser();
    if (!user) return '';
    const fullName = `${user.firstName} ${user.lastName}`.trim();
    return fullName || user.email;
  });

  setLanguage(lang: SupportedLanguage): void {
    this.languageService.use(lang);
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
