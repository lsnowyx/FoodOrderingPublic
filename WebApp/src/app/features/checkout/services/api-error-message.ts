import { HttpErrorResponse } from '@angular/common/http';

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function getValidationMessages(value: unknown): string[] {
  if (!isRecord(value)) {
    return [];
  }

  const messages: string[] = [];
  for (const fieldMessages of Object.values(value)) {
    if (!Array.isArray(fieldMessages)) {
      continue;
    }

    for (const message of fieldMessages) {
      if (typeof message === 'string' && message.trim().length > 0) {
        messages.push(message.trim());
      }
    }
  }

  return messages;
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return error instanceof Error && error.message === 'Malformed guest-checkout response.'
      ? 'The server returned an invalid checkout response. Your cart has been preserved.'
      : fallback;
  }

  if (error.status === 0) {
    return 'The server could not be reached. Check your connection and try again.';
  }

  if (typeof error.error === 'string' && error.error.trim().length > 0) {
    return error.error.trim();
  }

  if (isRecord(error.error)) {
    const detail = error.error['detail'];
    if (typeof detail === 'string' && detail.trim().length > 0) {
      return detail.trim();
    }

    const validationMessages = getValidationMessages(error.error['errors']);
    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }

    const title = error.error['title'];
    if (typeof title === 'string' && title.trim().length > 0) {
      return title.trim();
    }
  }

  return fallback;
}
