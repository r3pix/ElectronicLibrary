import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'electroniclibrary.theme';
const DARK_CLASS = 'dark-theme';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>('light');

  init(): void {
    this.apply(this.resolveInitialTheme());
  }

  toggle(): void {
    this.apply(this.theme() === 'dark' ? 'light' : 'dark');
  }

  private apply(theme: Theme): void {
    this.theme.set(theme);
    document.documentElement.classList.toggle(DARK_CLASS, theme === 'dark');
    localStorage.setItem(STORAGE_KEY, theme);
  }

  private resolveInitialTheme(): Theme {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark') return stored;

    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
