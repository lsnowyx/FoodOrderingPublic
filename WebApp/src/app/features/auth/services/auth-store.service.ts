import { Injectable, computed, signal } from '@angular/core';

import { AuthSession } from '../models/auth-session';
import { LoginResponse } from '../models/login-response';

export const AUTH_STORAGE_KEY = 'food-ordering-auth-v1';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function getLocalStorage(): Storage | null {
  try {
    return typeof localStorage === 'undefined' ? null : localStorage;
  } catch {
    return null;
  }
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split('.');
  if (parts.length !== 3 || parts.some(part => part.length === 0)) {
    return null;
  }

  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const payload: unknown = JSON.parse(atob(padded));
    return typeof payload === 'object' && payload !== null
      ? payload as Record<string, unknown>
      : null;
  } catch {
    return null;
  }
}

function readExpiry(token: string): number | null {
  const payload = decodeJwtPayload(token);
  const expiry = payload?.['exp'];
  return typeof expiry === 'number' && Number.isFinite(expiry) && expiry > 0
    ? expiry * 1000
    : null;
}

function parseSession(value: unknown): AuthSession | null {
  if (typeof value !== 'object' || value === null) {
    return null;
  }

  const session = value as Record<string, unknown>;
  const token = session['token'];
  const role = session['role'];
  const userId = session['userId'];
  const expiresAt = session['expiresAt'];

  if (
    typeof token !== 'string'
    || token.trim().length === 0
    || typeof role !== 'string'
    || role.trim().length === 0
    || typeof userId !== 'string'
    || !GUID_PATTERN.test(userId)
    || typeof expiresAt !== 'number'
    || !Number.isFinite(expiresAt)
  ) {
    return null;
  }

  const tokenExpiry = readExpiry(token);
  if (tokenExpiry === null || tokenExpiry !== expiresAt || expiresAt <= Date.now()) {
    return null;
  }

  return {
    token,
    role: role.trim(),
    userId,
    expiresAt
  };
}

function restoreSession(storage: Storage | null): AuthSession | null {
  if (storage === null) {
    return null;
  }

  try {
    const stored = storage.getItem(AUTH_STORAGE_KEY);
    if (stored === null) {
      return null;
    }

    const session = parseSession(JSON.parse(stored));
    if (session === null) {
      storage.removeItem(AUTH_STORAGE_KEY);
    }
    return session;
  } catch {
    try {
      storage.removeItem(AUTH_STORAGE_KEY);
    } catch {
      // Storage remains optional; an in-memory session can still be used.
    }
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly storage = getLocalStorage();
  private readonly writableSession = signal<AuthSession | null>(restoreSession(this.storage));

  readonly currentSession = this.writableSession.asReadonly();
  readonly token = computed(() => this.currentSession()?.token ?? null);
  readonly role = computed(() => this.currentSession()?.role ?? null);
  readonly userId = computed(() => this.currentSession()?.userId ?? null);
  readonly isAuthenticated = computed(() => {
    const session = this.currentSession();
    return session !== null && session.expiresAt > Date.now();
  });

  setSession(response: LoginResponse): boolean {
    const expiresAt = readExpiry(response.jwt);
    if (expiresAt === null || expiresAt <= Date.now()) {
      this.logout();
      return false;
    }

    const session = parseSession({
      token: response.jwt,
      role: response.role,
      userId: response.userId,
      expiresAt
    });
    if (session === null) {
      this.logout();
      return false;
    }

    this.writableSession.set(session);
    try {
      this.storage?.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
    } catch {
      // The authenticated session remains available in memory.
    }
    return true;
  }

  restoreSession(): void {
    this.writableSession.set(restoreSession(this.storage));
  }

  hasValidToken(): boolean {
    const session = this.currentSession();
    if (session === null || session.expiresAt <= Date.now()) {
      if (session !== null) {
        this.logout();
      }
      return false;
    }
    return true;
  }

  isTokenExpired(): boolean {
    const session = this.currentSession();
    return session === null || session.expiresAt <= Date.now();
  }

  getValidToken(): string | null {
    return this.hasValidToken() ? this.token() : null;
  }

  logout(): void {
    this.writableSession.set(null);
    try {
      this.storage?.removeItem(AUTH_STORAGE_KEY);
    } catch {
      // Storage cleanup failure must not keep the in-memory session active.
    }
  }
}
