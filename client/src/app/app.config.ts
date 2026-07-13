import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { provideTranslateService } from '@ngx-translate/core';
import { provideToastr } from 'ngx-toastr';
import { firstValueFrom } from 'rxjs';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AuthService } from './core/services/auth.service';
import { LanguageService } from './core/services/language.service';
import { ThemeService } from './core/services/theme.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    provideToastr(),
    provideTranslateService({ fallbackLang: 'en', lang: 'en' }),
    provideTranslateHttpLoader({ prefix: '/i18n/', suffix: '.json' }),
    provideAppInitializer(() => {
      inject(LanguageService).init();
      inject(ThemeService).init();

      const authService = inject(AuthService);
      if (!authService.isAuthenticated()) {
        return Promise.resolve();
      }
      // Tokens survived a reload — fetch the profile/roles before guards evaluate; drop back to
      // logged-out if they've gone stale server-side.
      return firstValueFrom(authService.loadCurrentUser()).catch(() => authService.logout());
    })
  ]
};
