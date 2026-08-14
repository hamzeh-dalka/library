import { HttpErrorResponse } from '@angular/common/http';

/**
 * Normalizes the various shapes the backend can send an error in:
 * - a plain string body (e.g. `BadRequest("Username already exists")`)
 * - the global ExceptionMiddleware JSON body: { statusCode, message, details }
 * - [ApiController] automatic validation ProblemDetails: { title, errors: { Field: [msg] } }
 * - a network/CORS failure, where err.error is a client-side ProgressEvent and err.status is 0
 */
export function extractErrorMessage(err: unknown, fallback: string): string {
  if (!(err instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (err.status === 0) {
    return 'Unable to reach the server. Please check your connection and try again.';
  }

  const body: unknown = err.error;

  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  if (body && typeof body === 'object') {
    const b = body as Record<string, unknown>;

    if (typeof b['message'] === 'string' && b['message'].trim()) {
      return b['message'];
    }

    if (b['errors'] && typeof b['errors'] === 'object') {
      const messages = Object.values(b['errors'] as Record<string, unknown>)
        .flat()
        .filter((m): m is string => typeof m === 'string' && m.trim().length > 0);
      if (messages.length) {
        return messages.join(' ');
      }
    }

    if (typeof b['title'] === 'string' && b['title'].trim()) {
      return b['title'];
    }
  }

  return fallback;
}
