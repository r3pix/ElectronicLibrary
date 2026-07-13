import { HttpErrorResponse } from '@angular/common/http';

// ASP.NET Core Identity's /register returns a ValidationProblem shaped as
// { title, status, errors: { [code]: string[] } } — flatten that into one readable string.
export function extractErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { errors?: Record<string, string[]>; title?: string } | null;
    if (body?.errors && typeof body.errors === 'object') {
      const messages = Object.values(body.errors).flat();
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }
    if (typeof body?.title === 'string') {
      return body.title;
    }
  }
  return fallback;
}
