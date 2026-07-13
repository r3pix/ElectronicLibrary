import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const STORAGE_KEY = 'electroniclibrary.lang';

export const SUPPORTED_LANGUAGES = ['en', 'pl'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);

  readonly currentLang = this.translate.currentLang;

  init(): void {
    this.translate.use(this.resolveInitialLanguage()).subscribe();
  }

  use(lang: SupportedLanguage): void {
    localStorage.setItem(STORAGE_KEY, lang);
    this.translate.use(lang).subscribe();
  }

  private resolveInitialLanguage(): SupportedLanguage {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (this.isSupported(stored)) return stored;

    const browserLang = navigator.language?.split('-')[0] ?? null;
    if (this.isSupported(browserLang)) return browserLang;

    return 'en';
  }

  private isSupported(lang: string | null): lang is SupportedLanguage {
    return !!lang && (SUPPORTED_LANGUAGES as readonly string[]).includes(lang);
  }
}
