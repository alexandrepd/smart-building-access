import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, StoredSession } from './auth.models';

const sessionKey = 'smart-building-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', credentials).pipe(
      tap((response) => this.saveSession(response)),
    );
  }

  getSession(): StoredSession | null {
    const raw = sessionStorage.getItem(sessionKey);
    if (!raw) return null;

    try {
      const session = JSON.parse(raw) as StoredSession;
      const expiry = new Date(session.expiresAt).getTime();
      if (!Number.isFinite(expiry) || expiry <= Date.now()) {
        this.clearSession();
        return null;
      }
      return session;
    } catch {
      this.clearSession();
      return null;
    }
  }

  getToken(): string | null {
    return this.getSession()?.accessToken ?? null;
  }

  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  private saveSession(response: LoginResponse): void {
    const claims = this.readClaims(response.accessToken);
    const session: StoredSession = {
      ...response,
      name: typeof claims['name'] === 'string' ? claims['name'] : 'Utilizador',
      role: typeof claims['role'] === 'string' ? claims['role'] : 'SecurityOperator',
    };
    sessionStorage.setItem(sessionKey, JSON.stringify(session));
  }

  private readClaims(token: string): Record<string, unknown> {
    try {
      const payload = token.split('.')[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
      const bytes = Uint8Array.from(atob(padded), (character) => character.charCodeAt(0));
      return JSON.parse(new TextDecoder().decode(bytes)) as Record<string, unknown>;
    } catch {
      return {};
    }
  }

  private clearSession(): void {
    sessionStorage.removeItem(sessionKey);
  }
}
