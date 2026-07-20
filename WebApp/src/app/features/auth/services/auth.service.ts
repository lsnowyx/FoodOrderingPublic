import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { LoginRequest } from '../models/login-request';
import { LoginResponse } from '../models/login-response';
import { RegisterRequest, RegisterResponse } from '../models/register-request';

const ACCOUNTS_API_URL = '/api/Accounts';
const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function parseLoginResponse(value: unknown): LoginResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed login response.');
  }

  const jwt = value['jwt'];
  const role = value['role'];
  const userId = value['userId'];

  if (
    typeof jwt !== 'string'
    || jwt.trim().length === 0
    || typeof role !== 'string'
    || role.trim().length === 0
    || typeof userId !== 'string'
    || !GUID_PATTERN.test(userId)
  ) {
    throw new Error('Malformed login response.');
  }

  return { jwt, role: role.trim(), userId };
}

function parseRegisterResponse(value: unknown): RegisterResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed registration response.');
  }

  const userName = value['userName'];
  const role = value['role'];
  if (
    typeof userName !== 'string'
    || userName.trim().length === 0
    || typeof role !== 'string'
    || role.trim().length === 0
  ) {
    throw new Error('Malformed registration response.');
  }

  return { userName: userName.trim(), role: role.trim() };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<unknown>(`${ACCOUNTS_API_URL}/Login`, request).pipe(
      map(parseLoginResponse)
    );
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<unknown>(`${ACCOUNTS_API_URL}/CreateUser`, request).pipe(
      map(parseRegisterResponse)
    );
  }
}
